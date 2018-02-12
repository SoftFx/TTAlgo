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

        public string AccountKey { get; }

        public IObservableListSource<BABotViewModel> Bots { get; }


        public BAAccountViewModel(AccountModelEntity entity, IDynamicDictionarySource<string, BABotViewModel> bots)
        {
            _entity = entity;
            AccountKey = BotAgentModel.GetAccountKey(_entity);

            Bots = bots.Where((k, b) => BotIsAttachedToAccount(b))
                .OrderBy((k, b) => b.InstanceId)
                .AsObservable();
        }


        public bool BotIsAttachedToAccount(BABotViewModel bot)
        {
            return bot.AccountKey == AccountKey;
        }
    }
}
