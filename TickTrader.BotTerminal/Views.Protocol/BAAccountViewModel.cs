using Caliburn.Micro;
using Machinarium.Qnil;
using TickTrader.Algo.Common.Info;

namespace TickTrader.BotTerminal
{
    internal class BAAccountViewModel : PropertyChangedBase
    {
        private AccountModelInfo _entity;


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

        public string AccountKey { get; }

        public IObservableList<BABotViewModel> Bots { get; }


        public BAAccountViewModel(AccountModelInfo entity, IVarSet<string, BABotViewModel> bots, BotAgentModel botAgent)
        {
            _entity = entity;

            Bots = bots.Where((k, b) => BotIsAttachedToAccount(b))
                .OrderBy((k, b) => b.InstanceId)
                .AsObservable();

            botAgent.AccountStateChanged += BotAgentOnAccountStateChanged;
        }


        public bool BotIsAttachedToAccount(BABotViewModel bot)
        {
            return bot.AccountKey == AccountKey;
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
