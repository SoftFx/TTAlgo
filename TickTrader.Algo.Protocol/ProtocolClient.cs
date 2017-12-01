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

        public IClientSessionSettings SessionSettings { get; }


        public ProtocolClient(IBotAgentClient agentClient, IClientSessionSettings settings)
        {
            AgentClient = agentClient;
            SessionSettings = settings;

            State = ClientState.Disconnected;
        }


        public void Connect()
        {
            if (State == ClientState.Connected)
                throw new Exception("Client is already connected");

            Listener = new BotAgentClientListener(AgentClient);

            ClientSession = new ClientSession(SessionSettings.ServerAddress, SessionSettings.CreateClientOptions())
            {
                Listener = Listener,
            };

            ClientSession.Connect(null, SessionSettings.ServerAddress);

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

        public void RequestAccounts(AccountListRequestEntity request)
        {
            ClientSession.SendAccountListRequest(null, request.ToMessage());
        }
    }
}
