using System.Net;
using System.Text.Json;

namespace WebsiteCertificateChecker;

public enum DnssecStatus
{
    Unknown,
    Valid,
    Unsigned,
    Invalid
}

public static class DnssecChecker
{
    private static readonly HttpClient Client = new()
    {
        Timeout = TimeSpan.FromSeconds(10),
        MaxResponseContentBufferSize = 1024 * 1024
    };

    public static async Task<DnssecStatus> GetStatusAsync(string host)
    {
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        return await GetStatusAsync(host, Client, timeout.Token).ConfigureAwait(false);
    }

    private static async Task<DnssecStatus> GetStatusAsync(
        string host, HttpClient client, CancellationToken cancellationToken)
    {
        // IP literals have no forward DNS zone to validate.
        if (IPAddress.TryParse(host.Trim('[', ']'), out _))
        {
            return DnssecStatus.Unknown;
        }

        var results = await Task.WhenAll(
            QueryAsync(host, "A", client, cancellationToken),
            QueryAsync(host, "AAAA", client, cancellationToken)).ConfigureAwait(false);

        if (results.Contains(DnssecStatus.Invalid)) return DnssecStatus.Invalid;
        if (results.Contains(DnssecStatus.Unknown)) return DnssecStatus.Unknown;
        return results.Contains(DnssecStatus.Unsigned) ? DnssecStatus.Unsigned : DnssecStatus.Valid;
    }

    private static async Task<DnssecStatus> QueryAsync(
        string host, string type, HttpClient client, CancellationToken cancellationToken)
    {
        try
        {
            var url = $"https://dns.google/resolve?name={Uri.EscapeDataString(host)}&type={type}" +
                      "&cd=0&do=1&edns_client_subnet=0.0.0.0%2F0";
            // Keep HTTPS certificate validation enabled: we trust this resolver's validation result.
            using var response = await client.GetAsync(url, cancellationToken).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
            using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken)
                .ConfigureAwait(false);
            return ParseResponse(document.RootElement);
        }
        catch (Exception exception) when (exception is HttpRequestException or OperationCanceledException
                                         or JsonException or NotSupportedException or IOException)
        {
            return DnssecStatus.Unknown;
        }
    }

    private static DnssecStatus ParseResponse(JsonElement response)
    {
        if (response.ValueKind != JsonValueKind.Object ||
            !response.TryGetProperty("Status", out var status) || status.ValueKind != JsonValueKind.Number ||
            !status.TryGetInt32(out var code) ||
            !response.TryGetProperty("CD", out var checkingDisabled) ||
            checkingDisabled.ValueKind != JsonValueKind.False ||
            !response.TryGetProperty("TC", out var truncated) || truncated.ValueKind != JsonValueKind.False)
        {
            return DnssecStatus.Unknown;
        }

        // SERVFAIL alone has many possible causes. Only explicit DNSSEC errors prove invalidity.
        if (response.TryGetProperty("extended_dns_errors", out var errors) &&
            errors.ValueKind == JsonValueKind.Array && errors.EnumerateArray().Any(error =>
                error.ValueKind == JsonValueKind.Object &&
                error.TryGetProperty("info_code", out var infoCode) && infoCode.ValueKind == JsonValueKind.Number &&
                infoCode.TryGetInt32(out var value) &&
                value is >= 6 and <= 12))
        {
            return DnssecStatus.Invalid;
        }

        if (errors.ValueKind == JsonValueKind.Array && errors.EnumerateArray().Any(error =>
                error.ValueKind == JsonValueKind.Object &&
                error.TryGetProperty("info_code", out var infoCode) && infoCode.ValueKind == JsonValueKind.Number &&
                infoCode.TryGetInt32(out var value) && value is 1 or 2 or 5) || code != 0 || !response.TryGetProperty("AD", out var authenticated))
        {
            return DnssecStatus.Unknown;
        }

        return authenticated.ValueKind switch
        {
            JsonValueKind.True => DnssecStatus.Valid,
            JsonValueKind.False => DnssecStatus.Unsigned,
            _ => DnssecStatus.Unknown
        };
    }
}
