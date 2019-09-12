using NetFwTypeLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using TickTrader.BotAgent.Configurator.Properties;

namespace TickTrader.BotAgent.Configurator
{
    public class PortsManager
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private const int MaxPort = (1 << 16) - 1;

        private readonly RegistryNode _currentAgent;
        private readonly CacheManager _cache;

        private readonly INetFwMgr _firewallManager;
        private readonly INetFwPolicy2 _firewallPolicy;

        private List<Uri> _busyUrls;

        public PortsManager(RegistryNode agent, CacheManager cache)
        {
            _currentAgent = agent;
            _cache = cache;
            _firewallManager = (INetFwMgr)Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FwMgr", false));
            _firewallPolicy = (INetFwPolicy2)Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FwPolicy2", true));
        }

        public void RegisterRuleInFirewall(string nameApp, string application, string porst)
        {
            string name = $"{nameApp} Access";

            INetFwRule firewallRule = null;

            bool newRule = false;

            try
            {
                firewallRule = _firewallPolicy.Rules.Item(name);
            }
            catch
            {
                firewallRule = (INetFwRule)Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FWRule", true));
                firewallRule.Name = name;
                newRule = true;
                _logger.Info(Resources.RuleNotFoundEx);
            }

            firewallRule.Protocol = (int)NET_FW_IP_PROTOCOL_.NET_FW_IP_PROTOCOL_TCP;
            firewallRule.Profiles = 7; // Profiles == ALL
            firewallRule.Action = NET_FW_ACTION_.NET_FW_ACTION_ALLOW;
            firewallRule.Description = "Bot Agent Custom Rules";
            firewallRule.LocalPorts = porst;
            firewallRule.Enabled = true;
            firewallRule.ApplicationName = application;

            if (newRule)
                _firewallPolicy.Rules.Add(firewallRule);
        }

        public void CheckPort(int port, string hostname = "current")
        {
            _busyUrls = UriChecker.GetEncodedUrls(_cache.BusyUrls);

            if (!CheckPortOpen(port, hostname))
            {
                string freePortMassage = string.Empty;

                for (int i = (port + 1) % MaxPort; i != port;)
                {
                    if (CheckPortOpen(i, hostname))
                    {
                        freePortMassage = $"Port {i} is free";
                        break;
                    }

                    i = (i + 1) % MaxPort;
                }

                if (string.IsNullOrEmpty(freePortMassage))
                    freePortMassage = "Free ports not found";

                var mes = $"Port {port} is not available. {freePortMassage}";

                _logger.Error(mes);
                throw new WarningException(mes);
            }
        }

        private bool CheckPortOpen(int port, string hostname)
        {
            var uri = new UriBuilder("https", hostname, port).Uri;

            var free = true;

            switch (uri.HostNameType)
            {
                case UriHostNameType.Dns:
                    var hosts = UriChecker.GetEncodedDnsHosts(uri);

                    free &= CheckIPv4(hosts.Item1, port);
                    free &= CheckIPv6(hosts.Item2, port);
                    break;

                case UriHostNameType.IPv4:
                    free &= CheckIPv4(hostname, port);
                    break;

                case UriHostNameType.IPv6:
                    free &= CheckIPv6(hostname, port);
                    break;

                default:
                    throw new ArgumentException($"Unknown hostname type: {uri}");
            }

            return free;
        }

        private bool CheckIPv4(string hostname, int port)
        {
            foreach (var tcp in ManagedIpHelper.GetExtendedTcpTable(true))
            {
                if (CheckLocalPoint(tcp.LocalEndPoint, hostname, port) && CheckActiveServiceState(tcp.State) && !IsAgentPort(hostname, port))
                    return false;
            }

            return true;
        }

        private bool CheckIPv6(string hostname, int port)
        {
            //var x = ManagedIpHelper.GetExtendedTcpTableIPv6(true);
            foreach (var listener in IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpListeners())
            {
                if (CheckLocalPoint(listener, hostname, port) && !IsAgentPort(hostname, port))
                    return false;
            }

            return true;
        }

        private bool CheckLocalPoint(IPEndPoint point, string hostname, int port)
        {
            return $"[{point.Address}]" == hostname && point.Port == port;
        }

        private bool CheckActiveServiceState(TcpState state)
        {
            return state == TcpState.Established || state == TcpState.Listen;
        }

        private bool IsAgentPort(string hostname, int port)
        {
            return _busyUrls.Where(u => u.Host == hostname && u.Port == port).Count() > 0;
        }
    }
}
