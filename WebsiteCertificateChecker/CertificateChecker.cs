using System.Net;
using System.Net.NetworkInformation;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;

namespace WebsiteCertificateChecker;

public static class CertificateChecker
{
    private static readonly TimeSpan ConnectTimeout = TimeSpan.FromSeconds(10);

    private static readonly Lazy<bool> CheckIPv6 = new(HasUsableIPv6Address);

    public static CertificateInfo GetCertificateInfo(string url)
    {
        return GetCertificateInfoAsync(url, CheckIPv6.Value).GetAwaiter().GetResult();
    }

    private static async Task<CertificateInfo> GetCertificateInfoAsync(string url, bool checkProtocols)
    {
        CertificateResult? ipv4 = null;
        CertificateResult? ipv6 = null;
        var dnssec = DnssecStatus.Unknown;

        if (Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            var dnssecTask = DnssecChecker.GetStatusAsync(uri.IdnHost);

            if (checkProtocols)
            {
                var ipv4Task = GetCertificateAsync(uri, AddressFamily.InterNetwork);
                var ipv6Task = GetCertificateAsync(uri, AddressFamily.InterNetworkV6);
                await Task.WhenAll(ipv4Task, ipv6Task).ConfigureAwait(false);
                ipv4 = await ipv4Task.ConfigureAwait(false);
                ipv6 = await ipv6Task.ConfigureAwait(false);
            }
            else
            {
                // Preserve the original connection selection on IPv4-only machines.
                ipv4 = await GetCertificateAsync(uri, null).ConfigureAwait(false);
            }

            dnssec = await dnssecTask.ConfigureAwait(false);
        }

        // Report the certificate that expires first so expiry filtering covers both protocols.
        var certificate = ipv4 == null ? ipv6 :
            ipv6 == null || ipv4.ExpirationDate <= ipv6.ExpirationDate ? ipv4 : ipv6;

        return new CertificateInfo
        {
            Url = url,
            Issuer = certificate?.Issuer,
            ExpirationDate = certificate?.ExpirationDate,
            Dnssec = dnssec,
            IPv4Success = checkProtocols ? ipv4 != null : null,
            IPv6Success = checkProtocols ? ipv6 != null : null,
            IPv4SslProtocol = checkProtocols ? ipv4?.SslProtocol : null,
            IPv6SslProtocol = checkProtocols ? ipv6?.SslProtocol : null,
            CertificatesMatch = ipv4 != null && ipv6 != null
                ? ipv4.Fingerprint == ipv6.Fingerprint
                : null
        };
    }

    private static async Task<CertificateResult?> GetCertificateAsync(Uri uri, AddressFamily? addressFamily)
    {
        try
        {
            using var timeout = new CancellationTokenSource(ConnectTimeout);
            using var client = addressFamily.HasValue ? new TcpClient(addressFamily.Value) : new TcpClient();
            await client.ConnectAsync(uri.IdnHost, uri.Port, timeout.Token).ConfigureAwait(false);

            await using var sslStream = new SslStream(client.GetStream());
            await sslStream.AuthenticateAsClientAsync(new SslClientAuthenticationOptions
            {
                TargetHost = uri.IdnHost
            }, timeout.Token).ConfigureAwait(false);

            if (sslStream.RemoteCertificate == null)
            {
                return null;
            }

            using var cert = new X509Certificate2(sslStream.RemoteCertificate);
            return new CertificateResult(ParseIssuer(cert.Issuer), cert.NotAfter,
                cert.GetCertHashString(HashAlgorithmName.SHA256), sslStream.SslProtocol);
        }
        catch
        {
            // DNS, connection and TLS failures are represented as an unavailable certificate.
            return null;
        }
    }

    private static bool HasUsableIPv6Address()
    {
        if (!Socket.OSSupportsIPv6)
        {
            return false;
        }

        try
        {
            return NetworkInterface.GetAllNetworkInterfaces().Any(network =>
                network.OperationalStatus == OperationalStatus.Up &&
                network.NetworkInterfaceType != NetworkInterfaceType.Loopback &&
                network.GetIPProperties().UnicastAddresses.Any(unicast =>
                {
                    var address = unicast.Address;
                    return address.AddressFamily == AddressFamily.InterNetworkV6 &&
                           !IPAddress.IsLoopback(address) &&
                           address is { IsIPv6LinkLocal: false, IsIPv6SiteLocal: false, IsIPv6Multicast: false, IsIPv4MappedToIPv6: false } &&
                           !address.Equals(IPAddress.IPv6Any);
                }));
        }
        catch (NetworkInformationException)
        {
            return false;
        }
    }

    private sealed record CertificateResult(
        string Issuer, DateTime ExpirationDate, string Fingerprint, SslProtocols SslProtocol);

    private static string ParseIssuer(string issuer)
    {
        var defaultValue = $"[{issuer}]";
        const string prefix = "O=";

        var prefixStart = issuer.IndexOf(prefix, StringComparison.Ordinal);

        if (prefixStart < 0)
        {
            return defaultValue;
        }

        var valueStart = prefixStart + prefix.Length;
        int valueEnd;

        if (issuer[valueStart] == '"')
        {
            valueStart++;

            valueEnd = issuer.IndexOf('"', valueStart);

            if (valueEnd < 0)
            {
                return defaultValue;
            }
        }
        else
        {
            valueEnd = issuer.IndexOf(',', valueStart);

            if (valueEnd < 0)
            {
                valueEnd = issuer.Length;
            }
        }

        return issuer[valueStart..valueEnd];
    }
}