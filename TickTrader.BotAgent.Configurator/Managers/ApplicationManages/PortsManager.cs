using NetFwTypeLib;
using System;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;

namespace TickTrader.BotAgent.Configurator
{
    public class PortsManager
    {
        private readonly INetFwMgr _firewallManager;
        private readonly ServiceManager _serviceManager;
        private readonly DefaultServiceSettingsModel _serviceModel;

        public PortsManager(ServiceManager service, DefaultServiceSettingsModel serviceModel)
        {
            _serviceManager = service;
            _serviceModel = serviceModel;
            _firewallManager = (INetFwMgr)Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FwMgr", false));
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

        public void RegisterPortInFirewall(int port, string name)
        {
            var portInst = (INetFwOpenPort)Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FWOpenPort", true));
            portInst.Port = port;
            portInst.Name = name;
            portInst.Enabled = true;

            _firewallManager?.LocalPolicy.CurrentProfile.GloballyOpenPorts.Add(portInst);
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
