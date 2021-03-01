using NetFwTypeLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using TickTrader.BotAgent.Configurator.IPHelper;
using TickTrader.BotAgent.Configurator.Properties;

namespace TickTrader.BotAgent.Configurator
{
    public class PortsManager
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private const int MaxPort = (1 << 16) - 1;

        private readonly RegistryNode _currentAgent;
        private readonly IPHelperWrapper _helper;
        private readonly ServiceManager _service;

        private readonly INetFwMgr _firewallManager;
        private readonly INetFwPolicy2 _firewallPolicy;

        public PortsManager(RegistryNode agent, ServiceManager service)
        {
            _helper = new IPHelperWrapper();

            _service = service;
            _currentAgent = agent;

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
            firewallRule.Description = "Algo Server Custom Rules";
            firewallRule.LocalPorts = porst;
            firewallRule.Enabled = true;
            firewallRule.ApplicationName = application;

            if (newRule)
                _firewallPolicy.Rules.Add(firewallRule);
        }

        public void CheckPort(int port, string hostname = "current")
        {
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
            foreach (var tcpRow in _helper.GetAllTCPv4Connections())
            {
                if (CheckLocalPoint(tcpRow.LocalEndPoint, $"[{hostname}]", port) && CheckActiveServiceState(tcpRow.State) && !IsAgentPort(tcpRow.ProcessId))
                    return false;
            }

            return true;
        }

        private bool CheckIPv6(string hostname, int port)
        {
            foreach (var tcpRow in _helper.GetAllTCPv6Connections())
            {
                if (CheckLocalPoint(tcpRow.LocalEndPoint, hostname, port) && CheckActiveServiceState(tcpRow.State) && !IsAgentPort(tcpRow.ProcessId))
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

        private bool IsAgentPort(uint processId)
        {
            //id 0 - System Idle Process
            //id 4 - System Process
            //id 8 - System Process (old version Windows)

            if (processId == 0 || processId == 4 || processId == 8)
                return false;

            var process = Process.GetProcessById((int)processId);

            return _service.IsServiceRunning && process.MainModule.FileName == _currentAgent.ExePath;
        }
    }
}
