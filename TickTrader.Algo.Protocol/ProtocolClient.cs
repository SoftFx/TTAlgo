using SoftFX.Net.BotAgent;
using System;
using TickTrader.Algo.Protocol.Lib;

namespace TickTrader.Algo.Protocol
{
    public enum ClientState { Connected, Disconnected, ConnectionError };


    public class ProtocolClient
    {
        internal ClientSession ClientSession { get; set; }

        internal BotAgentClientListener Listener { get; set; }


        public ClientState State { get; private set; }

        public IBotAgentClient AgentClient { get; }

        public string ServerAddress { get; }


        public ProtocolClient(IBotAgentClient agentClient, string serverAddress)
        {
            AgentClient = agentClient;

            Listener = new BotAgentClientListener(AgentClient);
            ServerAddress = serverAddress;

            var clientSessionOptions = new ClientSessionOptions(8443);
            clientSessionOptions.Log.Events = true;
            clientSessionOptions.Log.States = true;
            clientSessionOptions.Log.Messages = true;

            clientSessionOptions.ServerCertificateName = "certificate.pfx";
            clientSessionOptions.ConnectionType = SoftFX.Net.Core.ConnectionType.Secure;

            ClientSession = new ClientSession("Bot Agent Client", clientSessionOptions)
            {
                Listener = Listener,
            };
        }


        public void Connect()
        {
            if (State == ClientState.Connected)
                throw new Exception("Client is already connected");

            ClientSession.Connect(null, ServerAddress);

            State = ClientState.Connected;
        }

        public void Disconnect()
        {
            if (State == ClientState.Connected)
            {
                State = ClientState.Disconnected;

                ClientSession.Disconnect(null, "User request");
                ClientSession.Join();
            }
        }
    }
}
