﻿using Caliburn.Micro;
using Machinarium.Qnil;
using TickTrader.Algo.Common.Info;
using TickTrader.Algo.Protocol;

namespace TickTrader.BotTerminal
{
    internal class BotAgentViewModel : PropertyChangedBase
    {
        private string _server;
        private IVarSet<string, BABotViewModel> _bots;
        private IVarSet<AccountKey, BAAccountViewModel> _accounts;
        private IShell _shell;


        public BotAgentConnectionManager Connection { get; }

        public string Server
        {
            get { return _server; }
            set
            {
                if (_server == value)
                    return;

                _server = value;
                NotifyOfPropertyChange(nameof(Server));
            }
        }

        public string Status => Connection.Status;

        public IObservableList<BABotViewModel> Bots { get; }

        public IObservableList<BAAccountViewModel> Accounts { get; }


        public BotAgentViewModel(BotAgentConnectionManager connection, IShell shell)
        {
            Connection = connection;
            _shell = shell;

            _server = connection.Creds.ServerAddress;

            Connection.StateChanged += ConnectionOnStateChanged;

            _bots = Connection.BotAgent.Bots.Select((k, b) => new BABotViewModel(b, Connection.RemoteAgent, _shell));
            _accounts = Connection.BotAgent.Accounts.Select((k, a) => new BAAccountViewModel(a, _bots, Connection.RemoteAgent, _shell));

            Bots = _bots.OrderBy((k, b) => k).AsObservable();
            Accounts = _accounts.OrderBy((k, a) => k).AsObservable();
        }


        private void ConnectionOnStateChanged()
        {
            Server = Connection.Server;
            NotifyOfPropertyChange(nameof(Status));
        }
    }
}
