using NLog;
using SoftFX.Net.BotAgent;
using System;
using TickTrader.Algo.Protocol.Lib;

namespace TickTrader.Algo.Protocol
{
    public enum ServerStates { Started, Stopped }


    public class ProtocolServer
    {
        private readonly ILogger _logger;


        internal Server Server { get; set; }

        internal BotAgentServerListener Listener { get; set; }


        public ServerStates State { get; private set; }

        public IBotAgentServer AgentServer { get; }

        public IServerSettings Settings { get; }


        public ProtocolServer(IBotAgentServer agentServer, IServerSettings settings)
        {
            AgentServer = agentServer;
            Settings = settings;

            _logger = LoggerHelper.GetLogger("Protocol.Server", Settings.ProtocolSettings.LogDirectoryName, Settings.ServerName);

            State = ServerStates.Stopped;
        }


        public void Start()
        {
            if (State == ServerStates.Started)
                throw new Exception("Server is already started");

            Listener = new BotAgentServerListener(AgentServer, _logger);

            Server = new Server(Settings.ServerName, Settings.CreateServerOptions())
            {
                Listener = Listener,
            };

            Server.Start();

            State = ServerStates.Started;
        }

        public void Stop()
        {
            if (State == ServerStates.Started)
            {
                State = ServerStates.Stopped;

                Server.Stop(null);
                Server.Join();
            }
        }
    }
}
