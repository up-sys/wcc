using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;

namespace WebsiteCertificateChecker
{
    public static class CertificateChecker
    {
        public static CertificateInfo GetCertificateInfo(string url)
        {
            string? issuer = null;
            DateTime? expiration = null;

            try
            {
                var uri = new Uri(url);
                using var client = new TcpClient(uri.Host, uri.Port);
                using var sslStream = new SslStream(client.GetStream());

                sslStream.AuthenticateAsClient(uri.Host);

                if (sslStream.RemoteCertificate != null)
                {
                    var cert = new X509Certificate2(sslStream.RemoteCertificate);

                    issuer = ParseIssuer(cert.Issuer);
                    expiration = cert.NotAfter;
                }
            }
            catch
            {
            }

            return new CertificateInfo()
            {
                Url = url,
                Issuer = issuer,
                ExpirationDate = expiration,
            };
        }

        private static string ParseIssuer(string issuer)
        {
            var defaultValue = $"[{issuer}]";
            var prefix = "O=";

            var prefixStart = issuer.IndexOf(prefix);

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
}