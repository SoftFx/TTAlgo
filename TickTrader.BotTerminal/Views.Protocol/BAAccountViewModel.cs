﻿using Caliburn.Micro;
using Machinarium.Qnil;
using TickTrader.Algo.Common.Info;

namespace TickTrader.BotTerminal
{
    internal class BAAccountViewModel : PropertyChangedBase
    {
        private AccountModelInfo _entity;
        private RemoteAgent _remoteAgent;


        public string Login => _entity.Key.Login;

        public string Server => _entity.Key.Server;

        public string Status
        {
            get
            {
                if (_entity.LastError.Code == ConnectionErrorCodes.None)
                    return $"{_entity.ConnectionState}";
                if (_entity.LastError.Code == ConnectionErrorCodes.Unknown)
                    return $"{_entity.ConnectionState} - {_entity.LastError.TextMessage}";
                return $"{_entity.ConnectionState} - {_entity.LastError.Code}";
            }
        }

        public string AccountDisplayName { get; }

        public IObservableList<BABotViewModel> Bots { get; }


        public BAAccountViewModel(AccountModelInfo entity, IVarSet<string, BABotViewModel> bots, RemoteAgent remoteAgent)
        {
            _entity = entity;
            _remoteAgent = remoteAgent;

            AccountDisplayName = $"{_entity.Key.Server} - {_entity.Key.Login}";

            Bots = bots.Where((k, b) => BotIsAttachedToAccount(b))
                .OrderBy((k, b) => b.InstanceId)
                .AsObservable();

            remoteAgent.BotAgent.AccountStateChanged += BotAgentOnAccountStateChanged;
        }


        public void TestAccount()
        {
            _remoteAgent.TestAccount(_entity.Key);
        }

        public bool BotIsAttachedToAccount(BABotViewModel bot)
        {
            return bot.Account.Equals(_entity.Key);
        }


        private void BotAgentOnAccountStateChanged(AccountKey accountKey)
        {
            if (_entity.Key.Equals(accountKey))
            {
                NotifyOfPropertyChange(nameof(Status));
            }
        }
    }
}
