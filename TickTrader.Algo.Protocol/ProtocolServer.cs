using SoftFX.Net.BotAgent;
using System;
using TickTrader.Algo.Protocol.Lib;

namespace TickTrader.Algo.Protocol
{
    public enum ServerState { Started, Stopped }


    public class ProtocolServer
    {
        internal Server Server { get; set; }

        internal BotAgentServerListener Listener { get; set; }


        public ServerState State { get; private set; }

        public IBotAgentServer AgentServer { get; }


        public ProtocolServer(IBotAgentServer agentServer)
        {
            AgentServer = AgentServer;

            Listener = new BotAgentServerListener(AgentServer);

            var serverOptions = new ServerOptions(8443)
            {
                SessionThreadCount = 3
            };
            serverOptions.Log.Events = true;
            serverOptions.Log.States = true;
            serverOptions.Log.Messages = true;
            serverOptions.ConnectionType = SoftFX.Net.Core.ConnectionType.Secure;
            serverOptions.Certificate = new System.Security.Cryptography.X509Certificates.X509Certificate2(@"..\..\certificate.pfx");
            serverOptions.RequireClientCertificate = false;

            Server = new Server("Bot Agent Server", serverOptions)
            {
                Listener = Listener,
            };

            State = ServerState.Stopped;
        }


        public void Start()
        {
            if (State == ServerState.Started)
                throw new Exception("Server is already started");

            Server.Start();

            State = ServerState.Started;
        }

        public void Stop()
        {
            if (State == ServerState.Started)
            {
                State = ServerState.Stopped;

                Server.Stop(null);
                Server.Join();
            }
        }
    }
}
