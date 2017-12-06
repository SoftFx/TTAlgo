using NLog;
using SoftFX.Net.BotAgent;
using System;
using TickTrader.Algo.Protocol.Lib;

namespace TickTrader.Algo.Protocol
{
    public enum ServerStates { Started, Stopped, Faulted }


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
            try
            {
                if (State != ServerStates.Stopped)
                    throw new Exception($"Server is already {State}");

                Listener = new BotAgentServerListener(AgentServer, _logger);

                Server = new Server(Settings.ServerName, Settings.CreateServerOptions())
                {
                    Listener = Listener,
                };

                Server.Start();

                State = ServerStates.Started;
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to start protocol server: {ex.Message}");
                State = ServerStates.Faulted;
            }
        }

        public void Stop()
        {
            try
            {
                if (State == ServerStates.Started)
                {
                    State = ServerStates.Stopped;

                    Server.Stop(null);
                    Server.Join();
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to stop protocol server: {ex.Message}");
            }
        }
    }
}
