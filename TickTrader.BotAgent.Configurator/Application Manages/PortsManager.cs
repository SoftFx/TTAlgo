using NetFwTypeLib;
using System;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;

namespace TickTrader.BotAgent.Configurator
{
    public class PortsManager
    {
        //private string _localhost;
        private readonly INetFwMgr _firewallManager;

        public PortsManager()
        {
            _firewallManager = (INetFwMgr)Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FwMgr", false));
        }

        //public string LocalHost
        //{
        //    get
        //    {
        //        if (string.IsNullOrEmpty(_localhost))
        //            SetLocalHost();

        //        return _localhost;
        //    }
        //}

        public void CheckPortOpen(string urls)
        {
            var pair = GetHostAndPort(urls);

            CheckPortOpen(pair.Item1, pair.Item2);
        }

        public void CheckPortOpen(string hostname, int port)
        {
            if (hostname.ToLower().Trim('/') == "localhost")
                hostname = IPAddress.Loopback.ToString();

            //hostname = "10.9.14.74"; //from test
            //port = 52167;

            //using (var tcpClient = new TcpClient())
            //{
            //    try
            //    {
            //        tcpClient.Connect(hostname, port);
            //        tcpClient.Close();
            //    }
            //    catch
            //    {
            //        throw new WarningException($"Port {port} not avaible");
            //    }
            //}

            foreach (var tcp in IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpConnections())
            {
                if (tcp.LocalEndPoint.Port == port && CheckActiveServiceState(tcp))
                    throw new WarningException($"Port {port} not avaible");
            }

            foreach (var tcp in IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpListeners())
            {
                if (tcp.Port == port)
                    throw new WarningException($"Port {port} not avaible");
            }
        }

        private bool CheckActiveServiceState(TcpConnectionInformation tcp)
        {
            return tcp.State == TcpState.Established || tcp.State == TcpState.Listen;
        }

        public void RegistryPortInFirewall(int port, string name)
        {
            var portInst = (INetFwOpenPort)Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FWOpenPort", true));
            portInst.Port = port;
            portInst.Name = name + "new";
            portInst.Enabled = true;

            _firewallManager?.LocalPolicy.CurrentProfile.GloballyOpenPorts.Add(portInst);
        }

        public void RegistryApplicationInFirewall(string name, string path)
        {
            var applicationInst = (INetFwAuthorizedApplication)Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FWAuthorizedApplication", false));

            applicationInst.Name = name + "+";
            applicationInst.ProcessImageFileName = Path.Combine(path, "TestConfigurator.exe"); //Path.GetFileName(path)
            applicationInst.Enabled = true;

            _firewallManager.LocalPolicy.CurrentProfile.AuthorizedApplications.Add(applicationInst);
        }

        public Tuple<string, int> GetHostAndPort(string urls)
        {
            var parts = urls.Split(':');

            int n = parts.Length;

            if (n != 2 && n != 3)
                throw new Exception($"Incorrect urls {urls}");

            if (!int.TryParse(parts[n - 1].Trim('/'), out int port))
                throw new Exception($"Incorrect port {parts[n - 1]}");

            return new Tuple<string, int>(parts[n - 2].Trim('/'), port);
        }

        //private void SetLocalHost()
        //{
        //    var host = Dns.GetHostEntry(Dns.GetHostName());

        //    foreach (var ip in host.AddressList)
        //    {
        //        if (ip.AddressFamily == AddressFamily.InterNetwork)
        //        {
        //            _localhost = ip.ToString();
        //            return;
        //        }
        //    }

        //    throw new Exception("No network adapters with an IPv4 address in the system!");
        //}
    }
}
