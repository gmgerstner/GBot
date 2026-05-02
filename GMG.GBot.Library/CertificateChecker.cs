using System;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace GMG.GBot.Library
{
    internal class CertificateChecker
    {
        private static X509Certificate2 _serverCertificate;
      
        public static async Task<DateTime?> GetExpirationDate(string websiteUrl)
        {
            _serverCertificate = null;
            DateTime? expirationDate = null;
            var certificate = await GetCertificateAsync(websiteUrl);

            if (certificate != null)
            {
                expirationDate = certificate.NotAfter;
            }
            else
            {
                throw new Exception("Error reading site certificate");
            }
            return expirationDate;
        }

        private static async Task<X509Certificate2> GetCertificateAsync(string url)
        {
            try
            {
                using var handler = new HttpClientHandler();
                using var client = new HttpClient(handler);
                using var request = new HttpRequestMessage(HttpMethod.Get, url);
                client.BaseAddress = new($"https://{url}");

                handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, policyErrors) =>
                {
                    if (cert != null)
                    {
                        _serverCertificate = new X509Certificate2(cert);
                        return true;
                    }
                    return false;
                };
                await client.SendAsync(request);
                return _serverCertificate;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting certificate: {ex.Message}");
                return null;
            }
        }
    }
}
