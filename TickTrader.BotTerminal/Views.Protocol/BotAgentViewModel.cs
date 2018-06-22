using Caliburn.Micro;
using Machinarium.Qnil;
using TickTrader.Algo.Common.Info;
using TickTrader.Algo.Protocol;

namespace TickTrader.BotTerminal
{
    internal class BotAgentViewModel : PropertyChangedBase
    {
        private IShell _shell;


        public BotAgentConnectionManager Connection { get; }

        public string Status => Connection.Status;

        public AlgoAgentViewModel Agent { get; }

        public string Server { get; }


        public BotAgentViewModel(BotAgentConnectionManager connection, IShell shell)
        {
            Connection = connection;
            _shell = shell;

            Agent = new AlgoAgentViewModel(_shell, Connection.RemoteAgent);
            Server = Connection.Creds.ServerAddress;

            Connection.StateChanged += ConnectionOnStateChanged;
        }


        private void ConnectionOnStateChanged()
        {
            NotifyOfPropertyChange(nameof(Status));
        }
    }
}
