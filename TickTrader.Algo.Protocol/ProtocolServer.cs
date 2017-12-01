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

        public IServerSettings Settings { get; }


        public ProtocolServer(IBotAgentServer agentServer, IServerSettings settings)
        {
            AgentServer = agentServer;
            Settings = settings;

            State = ServerState.Stopped;
        }


        public void Start()
        {
            if (State == ServerState.Started)
                throw new Exception("Server is already started");

            Listener = new BotAgentServerListener(AgentServer);

            Server = new Server(Settings.ServerName, Settings.CreateServerOptions())
            {
                Listener = Listener,
            };

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
