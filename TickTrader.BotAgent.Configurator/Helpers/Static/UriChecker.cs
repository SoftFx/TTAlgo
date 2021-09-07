using System;
using System.Collections.Generic;
using System.Net;

namespace TickTrader.BotAgent.Configurator
{
    public class UriChecker
    {
        private const string Protocol = "https";

        public static List<Uri> GetEncodedUrls(List<Uri> urls)
        {
            var busyUrls = new List<Uri>(1 << 4);

            foreach (var url in urls)
            {
                if (url.HostNameType == UriHostNameType.Dns)
                {
                    if (url.IsLoopback)
                    {
                        busyUrls.Add(new UriBuilder(Protocol, IPAddress.Loopback.ToString(), url.Port).Uri);
                        busyUrls.Add(new UriBuilder(Protocol, IPAddress.IPv6Loopback.ToString(), url.Port).Uri);
                    }
                    else
                    {
                        busyUrls.Add(new UriBuilder(Protocol, IPAddress.Any.ToString(), url.Port).Uri);
                        busyUrls.Add(new UriBuilder(Protocol, IPAddress.IPv6Any.ToString(), url.Port).Uri);
                    }
                }
                else
                    busyUrls.Add(url);
            }

            return busyUrls;
        }

        public static Tuple<string, string> GetEncodedDnsHosts(Uri uri)
        {
            if (uri.HostNameType != UriHostNameType.Dns)
                return null;

            return uri.IsLoopback ? new Tuple<string, string>(IPAddress.Loopback.ToString(), $"[{IPAddress.IPv6Loopback}]") :
                new Tuple<string, string>(IPAddress.Any.ToString(), $"[{IPAddress.IPv6Any}]");
        }

        public static void CompareUriWithWarnings(Uri first, Uri second, string message = "")
        {
            var firHosts = GetEncodedDnsHosts(first);

            if (second.HostNameType == UriHostNameType.Dns)
            {
                var secHosts = GetEncodedDnsHosts(second);

                if (firHosts.Equals(secHosts) && first.Port == second.Port)
                    throw new WarningException(message);
            }
            else
            {
                if ((firHosts.Item1 == second.Host || firHosts.Item2 == second.Host) && first.Port == second.Port)
                    throw new WarningException(message);
            }
        }
    }
}
