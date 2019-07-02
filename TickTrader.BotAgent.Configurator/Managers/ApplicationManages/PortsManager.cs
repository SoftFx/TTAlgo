using NetFwTypeLib;
using System;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace TickTrader.BotAgent.Configurator
{
    public class PortsManager
    {
        private readonly INetFwMgr _firewallManager;
        private readonly ServiceManager _serviceManager;
        private readonly DefaultServiceSettingsModel _serviceModel;
        private readonly INetFwPolicy2 _firewallPolicy;

        public PortsManager(ServiceManager service, DefaultServiceSettingsModel serviceModel)
        {
            _serviceManager = service;
            _serviceModel = serviceModel;
            _firewallManager = (INetFwMgr)Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FwMgr", false));
            _firewallPolicy = (INetFwPolicy2)Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FwPolicy2", true));
        }

        public bool CheckPortOpen(int port, string hostname = "localhost", bool exception = true)
        {
            if (hostname.ToLower().Trim('/') == "localhost")
                hostname = IPAddress.Loopback.ToString();

            //hostname = "10.9.14.74"; //from test
            //port = 52167;

            foreach (var tcp in IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpConnections())
            {
                if (tcp.LocalEndPoint.Port == port && CheckActiveServiceState(tcp))
                    return !exception ? false : throw new WarningException($"Port {port} is not available");
            }

            foreach (var tcp in IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpListeners())
            {
                if (tcp.Port == port && !IsAgentService(port))
                    return !exception ? false : throw new WarningException($"Port {port} is not available");
            }

            return true;
        }

        private bool CheckActiveServiceState(TcpConnectionInformation tcp)
        {
            return tcp.State == TcpState.Established || tcp.State == TcpState.Listen;
        }

        private bool IsAgentService(int port)
        {
            return _serviceManager.IsServiceRunning && _serviceModel.ListeningPort == port;
        }

        public void RegisterRuleInFirewall(string nameApp, string application, string porst, string serviceName)
        {
            string name = $"{nameApp}Access";

            INetFwRule firewallRule = null;

            bool newRule = false;

            try
            {
                firewallRule = _firewallPolicy.Rules.Item(name);
            }
            catch (Exception ex)
            {
                firewallRule = (INetFwRule)Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FWRule", true));
                firewallRule.Name = name;
                newRule = true;
                Logger.Error(ex);
            }

            firewallRule.Protocol = (int)NET_FW_IP_PROTOCOL_.NET_FW_IP_PROTOCOL_TCP;
            firewallRule.Profiles = 7; // Profiles == ALL
            firewallRule.serviceName = serviceName;
            firewallRule.Action = NET_FW_ACTION_.NET_FW_ACTION_ALLOW;
            firewallRule.Description = "Bot Agent Custom Rules";
            firewallRule.LocalPorts = porst;
            firewallRule.Enabled = true;
            firewallRule.ApplicationName = application;

            if (newRule)
                _firewallPolicy.Rules.Add(firewallRule);
        }

        public void RegisterApplicationInFirewall(string name, string path)
        {
            var applicationInst = (INetFwAuthorizedApplication)Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FWAuthorizedApplication", false));

            applicationInst.Name = name + "+";
            applicationInst.ProcessImageFileName = Path.Combine(path, "TestConfigurator.exe"); //Path.GetFileName(path)
            applicationInst.Enabled = true;

            _firewallManager.LocalPolicy.CurrentProfile.AuthorizedApplications.Add(applicationInst);
        }
    }
}
