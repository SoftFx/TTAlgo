using Caliburn.Micro;
using Machinarium.Qnil;
using TickTrader.Algo.Protocol;

namespace TickTrader.BotTerminal
{
    internal class BotAgentViewModel : PropertyChangedBase
    {
        private string _server;


        public BotAgentConnectionManager Connection { get; }

        public string Server
        {
            get => _server;
            set
            {
                if (_server == value)
                    return;

                _server = value;
                NotifyOfPropertyChange(nameof(Server));
            }
        }

        public string Status => Connection.Status;

        public IObservableListSource<BotModelEntity> Bots => Connection.Bots;


        public BotAgentViewModel(BotAgentConnectionManager connection)
        {
            Connection = connection;

            _server = connection.Creds.ServerAddress;

            Connection.StateChanged += ConnectionOnStateChanged;
        }


        private void ConnectionOnStateChanged()
        {
            Server = Connection.Server;
            NotifyOfPropertyChange(nameof(Status));
        }
    }
}
