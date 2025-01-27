using Microsoft.Extensions.Configuration;
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace GMG.GBot.Library
{
    public class GoDaddyProvider
    {
        private string GoDaddyApiKey = "";
        private string GoDaddyApiSecret = "";
        private string AuthentactionHeaderValue = "";
        private string _baseUrl = "";
        private string _record;
        //private string _ipAddress;

        public GoDaddyProvider(string record, IConfiguration configuration)
        {
            _record = record;

            GoDaddyApiKey = configuration["GoDaddyProvider:GoDaddyApiKey"];
            GoDaddyApiSecret = configuration["GoDaddyProvider:GoDaddyApiSecret"];
            _baseUrl = configuration["GoDaddyProvider:BaseURL"];

            AuthentactionHeaderValue = "sso-key " + GoDaddyApiKey + ":" + GoDaddyApiSecret;
        }

        public async Task<string> GetCurrentIPAddress()
        {
            using HttpClient client = new();
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Add("Authorization", AuthentactionHeaderValue);
            var url = $"{_baseUrl}/{_record}";
            var response = await client.GetAsync(url);

            var stream = response.Content.ReadAsStream();
            var buffer = new byte[stream.Length];
            stream.Read(buffer, 0, (int)stream.Length);
            var output = Encoding.ASCII.GetString(buffer);

            if (!response.IsSuccessStatusCode) throw new Exception(output);

            var obj = JsonSerializer.Deserialize<DomainRecordResponse[]>(output);

            return obj[0].data;
        }

        public async Task UpdateIPAddress(string ipAddress)
        {
            using HttpClient client = new();
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Add("Authorization", AuthentactionHeaderValue);
            await ProcessRepositoriesAsync(client, ipAddress);
        }

        private async Task ProcessRepositoriesAsync(HttpClient client, string ipAddress)
        {
            string body =
                "		[   \r\n"
                + "		  {\r\n"
                + $"		    \"data\": \"{ipAddress}\",\r\n"
                + "		    \"ttl\": 600,\r\n"
                + "		    \"type\": \"A\"\r\n"
                + "		  }\r\n"
                + "		]\r\n";

            StringContent content = new StringContent(body, Encoding.UTF8, "application/json");
            var url = $"{_baseUrl}/{_record}";
            var response = await client.PutAsync(url, content);

            var stream = response.Content.ReadAsStream();
            var buffer = new byte[stream.Length];
            stream.Read(buffer, 0, (int)stream.Length);
            var output = Encoding.ASCII.GetString(buffer);

            if (!response.IsSuccessStatusCode) throw new Exception(output);
        }
    }

    public class DomainRecordResponse
    {
        public string data { get; set; }
        public string name { get; set; }
        public int ttl { get; set; }
        public string type { get; set; }
    }
}
