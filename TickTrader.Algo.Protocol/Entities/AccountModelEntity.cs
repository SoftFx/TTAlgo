using SoftFX.Net.BotAgent;

namespace TickTrader.Algo.Protocol
{
    public class AccountModelEntity : IProtocolEntity<AccountModel>
    {
        public AccountKeyEntity Key { get; set; }

        public BotModelEntity[] Bots { get; set; }


        internal void UpdateModel(AccountModel model)
        {
            Key.UpdateModel(model.Key);
            model.Bots.Resize(Bots.Length);
            for (var i = 0; i < Bots.Length; i++)
            {
                Bots[i].UpdateModel(model.Bots[i]);
            }

        }

        internal void UpdateSelf(AccountModel model)
        {
            Key.UpdateSelf(model.Key);
            Bots = new BotModelEntity[model.Bots.Length];
            for (var i = 0; i < model.Bots.Length; i++)
            {
                Bots[i] = new BotModelEntity();
                Bots[i].UpdateSelf(model.Bots[0]);
            }
        }


        void IProtocolEntity<AccountModel>.UpdateModel(AccountModel model)
        {
            UpdateModel(model);
        }

        void IProtocolEntity<AccountModel>.UpdateSelf(AccountModel model)
        {
            UpdateSelf(model);
        }
    }
}
