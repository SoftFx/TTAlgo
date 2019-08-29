﻿using NetFwTypeLib;
using System;
using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;

namespace TickTrader.BotAgent.Configurator
{
    public class PortsManager
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private const int MaxPort = 1 << 16;

        private readonly INetFwMgr _firewallManager;
        private readonly ServiceManager _serviceManager;
        private readonly INetFwPolicy2 _firewallPolicy;

        private RegistryNode _currentAgent;

        public PortsManager(ServiceManager service, RegistryNode agent)
        {
            _serviceManager = service;
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
                _logger.Info("Firewall: Rule not found. A new rule will be created.");
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

        public void CheckPort(int port, int nativePort, string hostname = null)
        {
            if (!CheckPortOpen(port, hostname, nativePort))
                FindFreePort(port, nativePort, hostname);
        }

        private void FindFreePort(int port, int nativePort, string hostname)
        {
            string freePortMassage = string.Empty;

            for (int i = (port + 1) % MaxPort; i != port;)
            {
                if (CheckPortOpen(i, hostname, nativePort))
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

        private bool CheckPortOpen(int port, string hostname, int nativePort)
        {
            if (hostname != null && hostname.ToLower().Trim('/') == "localhost")
                hostname = IPAddress.Loopback.ToString();

            foreach (var tcp in ManagedIpHelper.GetExtendedTcpTable(true))
            {
                if (CheckAdress(tcp, hostname) && tcp.LocalEndPoint.Port == port && CheckActiveServiceState(tcp.State) && !IsAgentService(tcp.ProcessId))
                    return false;
            }

            return true;
        }

        private bool CheckAdress(TcpRow tcp, string hostname)
        {
            return hostname != null ? tcp.LocalEndPoint.Address.ToString() == hostname : true;
        }

        private bool CheckActiveServiceState(TcpState state)
        {
            return state == TcpState.Established || state == TcpState.Listen;
        }

        private bool IsAgentService(int id)
        {
            var service = Process.GetProcessById(id);

            return _serviceManager.IsServiceRunning && service.MainModule.FileName == _currentAgent.ExePath;
        }
    }
}
