using Caliburn.Micro;
using Machinarium.Qnil;
using TickTrader.Algo.Protocol;

namespace TickTrader.BotTerminal
{
    internal class BAAccountViewModel : PropertyChangedBase
    {
        private AccountModelEntity _entity;


        public string Login => _entity.Login;

        public string Server => _entity.Server;

        public string Status
        {
            get
            {
                if (_entity.LastError.Code == ConnectionErrorCode.None)
                    return $"{_entity.ConnectionState}";
                if (_entity.LastError.Code == ConnectionErrorCode.Unknown)
                    return $"{_entity.ConnectionState} - {_entity.LastError.Text}";
                return $"{_entity.ConnectionState} - {_entity.LastError.Code}";
            }
        }

        public string AccountKey { get; }

        public IObservableListSource<BABotViewModel> Bots { get; }


        public BAAccountViewModel(AccountModelEntity entity, IDynamicDictionarySource<string, BABotViewModel> bots, BotAgentModel botAgent)
        {
            _entity = entity;
            AccountKey = BotAgentModel.GetAccountKey(_entity);

            Bots = bots.Where((k, b) => BotIsAttachedToAccount(b))
                .OrderBy((k, b) => b.InstanceId)
                .AsObservable();

            botAgent.AccountStateChanged += BotAgentOnAccountStateChanged;
        }


        public bool BotIsAttachedToAccount(BABotViewModel bot)
        {
            return bot.AccountKey == AccountKey;
        }


        private void BotAgentOnAccountStateChanged(string accountKey)
        {
            if (AccountKey == accountKey)
            {
                NotifyOfPropertyChange(nameof(Status));
            }
        }
    }
}
