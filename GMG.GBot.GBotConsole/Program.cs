using GMG.GBot.Library;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace GMG.GBot.GBotConsole
{
    class Program
    {
        public static Task Main(string[] args)
        {
            IConfiguration configuration = new ConfigurationBuilder()
                        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                        .Build();
            return new GBotService(configuration).Start();
        }
    }
}
