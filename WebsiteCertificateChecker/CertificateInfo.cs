using System.Security.Authentication;

namespace WebsiteCertificateChecker;

public class CertificateInfo
{
    private const string UnsupportedProtocol = " N/A";
    private const int ProtocolInfoWidth = 8;

    public string Url { get; init; } = null!;
    public string? Issuer { get; init; }
    public bool IsIssuerTrusted { get; init; } = true;
    public DateTime? ExpirationDate { get; init; }

    public bool? IPv4Success { get; init; }
    public bool? IPv6Success { get; init; }
    public SslProtocols? IPv4SslProtocol { get; init; }
    public SslProtocols? IPv6SslProtocol { get; init; }
    public bool? CertificatesMatch { get; init; }
    public DnssecStatus Dnssec { get; init; }

    private string ExpirationDateFormatted => ExpirationDate?.ToString("yyyy-MM-dd") ?? "N/A";
    private int DaysRemainingToExpire => (ExpirationDate.GetValueOrDefault() - DateTime.Now).Days;
    private ConsoleColor TextColor => DaysRemainingToExpire switch
    {
        < 14 => ConsoleColor.Red,
        < 30 => ConsoleColor.Yellow,
        _ => ConsoleColor.Green
    };

    private static string FormatProtocol(SslProtocols? sslProtocol)
    {
        var version = sslProtocol?.ToString() switch
        {
            "Ssl2" => "SSL 2.0",
            "Ssl3" => "SSL 3.0",
            "Tls" => "TLS 1.0",
            "Tls11" => "TLS 1.1",
            "Tls12" => "TLS 1.2",
            "Tls13" => "TLS 1.3",
            null => null,
            var other => other
        };

        return version == null ? string.Empty : $" {version}";
    }

    private static Text FormatProtocolText(SslProtocols? sslProtocol)
    {
        ConsoleColor? color = sslProtocol?.ToString() switch
        {
            "Ssl2" or "Ssl3" or "Tls" or "Tls11" => ConsoleColor.Red,
            "Tls12" => ConsoleColor.Yellow,
            "Tls13" => ConsoleColor.Green,
            _ => null
        };

        return new Text(FormatProtocol(sslProtocol).PadRight(ProtocolInfoWidth), color);
    }

    public Row ToRow()
    {
        var row = new Row();

        row.AddCell(Url);
        row.AddCell("Expiration date: " + new Text($"{ExpirationDateFormatted}", TextColor));

        if (ExpirationDate != null)
        {
            row.AddCell("Remaining days: " + new Text($"{DaysRemainingToExpire}", TextColor));
        }
        else
        {
            row.AddEmpty();
        }

        if (Issuer != null)
        {
            row.AddCell(IsIssuerTrusted
                ? $"Issuer: {Issuer}"
                : "Issuer: " + new Text($"NOT TRUSTED {Issuer}", ConsoleColor.Red));
        }
        else
        {
            row.AddEmpty();
        }

        if (IPv4Success.HasValue && IPv6Success.HasValue)
        {
            row.AddCell(new Text("IPv4", IPv4Success.Value ? ConsoleColor.Green : ConsoleColor.Red)
                        + (IPv4Success.Value
                            ? FormatProtocolText(IPv4SslProtocol)
                            : new Text(UnsupportedProtocol.PadRight(ProtocolInfoWidth), ConsoleColor.Red)));

            var ipv6 = new Text("IPv6", IPv6Success.Value ? ConsoleColor.Green : ConsoleColor.Red)
                       + (IPv6Success.Value
                           ? FormatProtocolText(IPv6SslProtocol)
                           : new Text(UnsupportedProtocol.PadRight(ProtocolInfoWidth), ConsoleColor.Red));

            if (CertificatesMatch == false)
            {
                ipv6 += " " + new Text("Certificates differ", ConsoleColor.Red);
            }

            row.AddCell(ipv6);
        }

        row.AddCell(Dnssec switch
        {
            DnssecStatus.Valid => new Text("DNSSEC valid", ConsoleColor.Green),
            DnssecStatus.Unsigned => new Text("DNSSEC unsigned", ConsoleColor.Gray),
            DnssecStatus.Invalid => new Text("DNSSEC invalid", ConsoleColor.Red),
            _ => new Text("DNSSEC unknown", ConsoleColor.Yellow)
        });

        return row;
    }
}
