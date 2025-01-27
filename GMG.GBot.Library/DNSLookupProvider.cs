using Serilog;
using System.Collections.Generic;
using System.Net;

namespace GMG.GBot.Library
{
    internal class DNSLookupProvider
    {
        public List<string> GetIPAddresses(string dnsname)
        {
            try
            {
                List<string> results = [];

                IPHostEntry ipEntry;
                IPAddress[] ipAddr;

                ipEntry = Dns.GetHostEntry(dnsname);
                ipAddr = ipEntry.AddressList;

                int i = 0;
                int len = ipAddr.Length;
                for (i = 0; i < len; i++)
                {
                    results.Add(ipAddr[i].ToString());
                }
                return results;

            }
            catch (System.Exception ex)
            {
                Log.Error(ex, ex.ToString());
                throw;
            }
        }
    }
}
