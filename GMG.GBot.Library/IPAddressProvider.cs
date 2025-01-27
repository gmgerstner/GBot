using Microsoft.Extensions.Configuration;
using Serilog;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace GMG.GBot.Library
{
    internal class IPAddressProvider
    {
        public string RequestUri { get; }

        public IPAddressProvider(IConfiguration configuration)
        {
            RequestUri = configuration["IPAddressProvider:RequestUri"];
        }

        public async Task<string> GetIPAddress()
        {
            try
            {
                var client = new HttpClient();
                client.Timeout = TimeSpan.FromSeconds(600);
                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri(RequestUri),
                };
                using (var response = await client.SendAsync(request))
                {
                    response.EnsureSuccessStatusCode();
                    var body = await response.Content.ReadAsStringAsync();
                    return body;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, ex.ToString());
                throw;
            }
        }
    }
}
