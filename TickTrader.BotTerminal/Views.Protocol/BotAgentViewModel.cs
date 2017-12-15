using Caliburn.Micro;
using Machinarium.Qnil;
using TickTrader.Algo.Protocol;

namespace TickTrader.BotTerminal
{
    internal class BotAgentViewModel : PropertyChangedBase
    {
        private string _server;
        private string _status;


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

        public string Status
        {
            get => _status;
            set
            {
                if (_status == value)
                    return;

                _status = value;
                NotifyOfPropertyChange(nameof(Status));
            }
        }

        public IObservableListSource<BotModelEntity> Bots => Connection.Bots;


        public BotAgentViewModel(BotAgentConnectionManager connection)
        {
            Connection = connection;

            _server = connection.Creds.ServerAddress;
            _status = connection.Status;

            Connection.StateChanged += ConnectionOnStateChanged;
        }

        private void ConnectionOnStateChanged()
        {
            Server = Connection.Server;
            Status = Connection.Status;
        }
    }
}
