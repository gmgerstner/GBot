using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace GMG.GBot.Library
{
    public class GBotService
    {
        private string Token = "";
        private ulong GeneralChannelID = 0;
        private string DNSName = "";
        private string RootRecord = "@";
        private bool useGoDadddyProvider = false;
        private bool useDNSLookupProvider = false;

        private DiscordSocketClient _client;
        private IPAddressProvider ipaddressProvider;
        private GoDaddyProvider goDaddyProvider;
        private DNSLookupProvider lookupProvider;

        public GBotService(IConfiguration configuration)
        {
            Token = configuration["GBotService:Token"];
            GeneralChannelID = ulong.Parse(configuration["GBotService:GeneralChannelID"]);
            DNSName = configuration["GBotService:DNSName"];
            RootRecord = configuration["GBotService:RootRecord"];
            useGoDadddyProvider = bool.Parse(configuration["GBotService:UseGoDadddyProvider"]);
            useDNSLookupProvider = bool.Parse(configuration["GBotService:UseDNSLookupProvider"]);

            IPAddressProvider ipaddressProvider = new(configuration);
            GoDaddyProvider goDaddyProvider = new(RootRecord, configuration);
            DNSLookupProvider lookupProvider = new();
        }

        public async Task Start(CancellationToken? stoppingToken = null)
        {
            Serilog.Log.Information("GBot Starting");

            _client = new DiscordSocketClient();

            _client.Log += Log;
            _client.MessageReceived += HandleCommandAsync;

            await _client.LoginAsync(TokenType.Bot, Token);
            await _client.StartAsync();

            await Process(_client);

            // Block this task until the program is closed.
            if (stoppingToken != null)
                await Task.Delay((int)TimeSpan.FromMinutes(1).TotalMilliseconds, stoppingToken.Value);
            else
                await Task.Delay(-1);
        }

        private async Task HandleCommandAsync(SocketMessage arg)
        {
            try
            {
                // Bail out if it's a System Message.
                var msg = arg as SocketUserMessage;
                if (msg == null) return;

                // We don't want the bot to respond to itself or other bots.
                if (msg.Author.Id == _client.CurrentUser.Id || msg.Author.IsBot) return;

                // Create a number to track where the prefix ends and the command begins
                int pos = 0;
                // Replace the '!' with whatever character
                // you want to prefix your commands with.
                // Uncomment the second half if you also want
                // commands to be invoked by mentioning the bot instead.
                if (msg.HasCharPrefix('!', ref pos) /* || msg.HasMentionPrefix(_client.CurrentUser, ref pos) */)
                {
                    //// Create a Command Context.
                    //var context = new SocketCommandContext(_client, msg);

                    //// Execute the command. (result does not indicate a return value, 
                    //// rather an object stating if the command executed successfully).
                    //var result = await _commands.ExecuteAsync(context, pos, _services);

                    //// Uncomment the following lines if you want the bot
                    //// to send a message if it failed.
                    //// This does not catch errors from commands with 'RunMode.Async',
                    //// subscribe a handler for '_commands.CommandExecuted' to see those.
                    ////if (!result.IsSuccess && result.Error != CommandError.UnknownCommand)
                    ////    await msg.Channel.SendMessageAsync(result.ErrorReason);

                    switch (msg.Content.Trim().ToLower())
                    {
                        case "!ipaddress":
                            {
                                var ipaddress = await ipaddressProvider.GetIPAddress();
                                await SendMessage(_client, $"IP Address: {ipaddress}");
                                break;
                            }
                        case "!dns":
                            if (useGoDadddyProvider)
                            {
                                var dnsip = await goDaddyProvider.GetCurrentIPAddress();
                                await SendMessage(_client, dnsip);
                            }
                            else if (useDNSLookupProvider)
                            {
                                var dnsip_list = lookupProvider.GetIPAddresses(DNSName);
                                var dnsip = string.Join("\r\n", dnsip_list);
                                await SendMessage(_client, dnsip);
                            }
                            break;
                        case "!status":
                            if (useGoDadddyProvider)
                            {
                                var ipaddress = await ipaddressProvider.GetIPAddress();
                                var dnsip = await goDaddyProvider.GetCurrentIPAddress();
                                await DNSCheckMessage(_client, ipaddress, dnsip);
                            }
                            else if (useDNSLookupProvider)
                            {
                                var ipaddress = await ipaddressProvider.GetIPAddress();
                                var dnsip_list = lookupProvider.GetIPAddresses(DNSName);
                                var dnsip = string.Join("\r\n", dnsip_list);
                                await DNSCheckMessage(_client, ipaddress, dnsip);
                            }
                            break;
                        case "!update":
                            if (useGoDadddyProvider)
                            {
                                var ipaddress = await ipaddressProvider.GetIPAddress();
                                var dnsip = await goDaddyProvider.GetCurrentIPAddress();

                                if (!dnsip.Contains(ipaddress))
                                {
                                    //update IP
                                    await goDaddyProvider.UpdateIPAddress(ipaddress);
                                    var message = $"DNS Udpdated from {dnsip} to {ipaddress}.";
                                    await SendMessage(_client, message);
                                }
                                else
                                {
                                    var message = $"DNS is already up to date: {dnsip}.";
                                    await SendMessage(_client, message);
                                }
                            }
                            else if (useDNSLookupProvider)
                            {
                                var txt = "The update command is not supported the the lookUpProvider";
                                await SendMessage(_client, txt);
                            }
                            break;
                        case "!help":
                            {
                                var txt = "";
                                txt += "ipaddress: Get current IP Address\r\n";
                                txt += "dns: Get current DNS value\r\n";
                                if (useGoDadddyProvider) txt += "update: Update IP Address\r\n";
                                txt += "status: Report full status\r\n";
                                await SendMessage(_client, txt);
                                break;
                            }
                        default:
                            //ignore request...
                            await msg.Channel.SendMessageAsync($"Command not recognized: {msg.Content}");
                            break;
                    }
                }

            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "Error handling command.");
                throw;
            }
        }

        private async Task Process(DiscordSocketClient client)
        {
            await Task.Delay(2000);
            await SendMessage(_client, "GBot service started.");
            do
            {
                Thread.Sleep(MillisecondsToNextHour());
                string source = "";
                try
                {
                    source = "GetIPAddress()";
                    var ipaddress = await ipaddressProvider.GetIPAddress();
                    source = "GetIPAddresses(DNSName)";
                    string dnsip = "";
                    if (useGoDadddyProvider)
                    {
                        dnsip = await goDaddyProvider.GetCurrentIPAddress();
                    }
                    else if (useDNSLookupProvider)
                    {
                        dnsip = lookupProvider.GetIPAddresses(DNSName).FirstOrDefault() ?? "";
                    }
                    source = "";
                    if (!dnsip.Contains(ipaddress))
                    {
                        var message = "The external IP address and the currently configured DNS IP address do not match: \r\n";
                        await SendMessage(client, message);
                        await DNSCheckMessage(client, ipaddress, dnsip);

                        //update IP
                        if (useGoDadddyProvider)
                        {
                            await goDaddyProvider.UpdateIPAddress(ipaddress);
                            message = $"DNS Udpdated from {dnsip} to {ipaddress}.";
                            await SendMessage(client, message);
                        }
                        else
                        {
                            message = $"The domain name {DNSName} needs to be updated to {ipaddress}";
                        }
                    }
                }
                catch (TaskCanceledException ex2)
                {
                    var msg = "Unable to send message. Connection was canceled.\r\n" + ex2.Message;
                    if (source != "") msg += $" in {source}";
                    Serilog.Log.Error(ex2, msg);
                    // ignore this failure and continue
                }
                catch (HttpRequestException ex2)
                {
                    var msg = "Unable to send message. The SSL connection could not be established. \r\n" + ex2.Message;
                    if (source != "") msg += $" in {source}";
                    Serilog.Log.Error(ex2, msg);
                    // ignore this failure and continue
                }
                catch (Exception ex)
                {
                    try
                    {
                        await SendMessage(_client, ex.Message);
                    }
                    catch (Exception ex2)
                    {
                        var msg = "Unable to send message: \r\n" + ex.Message;
                        if (source != "") msg += $" in {source}";
                        Serilog.Log.Error(ex2, msg);
                        throw;
                    }
                }
            } while (true);
        }

        private async Task DNSCheckMessage(DiscordSocketClient client, string ipaddress, string dnsip)
        {
            var message = "";
            message += $"-Current IP Address: {ipaddress}\r\n";
            message += $"-DNS Configured for {DNSName}:{dnsip}";
            await SendMessage(client, message);
        }

        private static int MillisecondsToNextHour()
        {
            var now = DateTime.Now;
            var next = now.Date.AddHours(now.Hour + 1);
            int timeUntilNextHour = (int)((next - now).TotalMilliseconds);
            return timeUntilNextHour;
        }

        private async Task SendMessage(DiscordSocketClient client, string message)
        {
            try
            {
                var channel = client.GetChannel(GeneralChannelID) as IMessageChannel;
                await channel.SendMessageAsync(message);

            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "Error sending message to Discord.");
                throw;
            }
        }

        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }
    }
}
