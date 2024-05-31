using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;

namespace WebsiteCertificateChecker
{
    public static class CertificateChecker
    {
        public static DateTime? GetCertificateExpirationDate(string url)
        {
            try
            {
                var uri = new Uri(url);
                using var client = new TcpClient(uri.Host, uri.Port);
                using var sslStream = new SslStream(client.GetStream());

                sslStream.AuthenticateAsClient(uri.Host);

                return sslStream.RemoteCertificate == null ? null : GetCertificateExpiration(sslStream.RemoteCertificate);
            }
            catch
            {
                return null;
            }
        }

        private static DateTime? GetCertificateExpiration(X509Certificate certificate)
        {
            var cert = new X509Certificate2(certificate);
            return cert.NotAfter;
        }
    }
}