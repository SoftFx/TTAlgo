using Caliburn.Micro;
using Machinarium.Qnil;
using TickTrader.Algo.Common.Info;
using TickTrader.Algo.Core.Metadata;
using TickTrader.Algo.Protocol;

namespace TickTrader.BotTerminal
{
    internal class BotAgentViewModel : PropertyChangedBase, IDropHandler
    {
        private AlgoEnvironment _algoEnv;


        public BotAgentConnectionManager Connection { get; }

        public string Status => Connection.Status;

        public AlgoAgentViewModel Agent { get; }

        public string Server { get; }


        public BotAgentViewModel(BotAgentConnectionManager connection, AlgoEnvironment algoEnv)
        {
            Connection = connection;
            _algoEnv = algoEnv;

            Agent = new AlgoAgentViewModel(Connection.RemoteAgent, _algoEnv);
            Server = Connection.Creds.ServerAddress;

            Connection.StateChanged += ConnectionOnStateChanged;
        }


        public void Drop(object o)
        {
            var algoBot = o as AlgoPluginViewModel;
            if (algoBot != null && algoBot.Type == AlgoTypes.Robot)
            {
                Agent.OpenBotSetup(algoBot.Info);
            }
        }

        public bool CanDrop(object o)
        {
            return o is AlgoPluginViewModel;
        }


        private void ConnectionOnStateChanged()
        {
            NotifyOfPropertyChange(nameof(Status));
        }
    }
}
