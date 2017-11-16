using SoftFX.Net.BotAgent;

namespace TickTrader.Algo.Protocol
{
    public class AccountKeyEntity : IProtocolEntity<AccountKey>
    {
        public string Login { get; internal set; }

        public string Server { get; internal set; }


        internal void UpdateModel(AccountKey model)
        {
            model.Login = Login;
            model.Server = Server;
        }

        internal void UpdateSelf(AccountKey model)
        {
            Login = model.Login;
            Server = model.Server;
        }


        void IProtocolEntity<AccountKey>.UpdateModel(AccountKey model)
        {
            UpdateModel(model);
        }

        void IProtocolEntity<AccountKey>.UpdateSelf(AccountKey model)
        {
            UpdateSelf(model);
        }
    }
}
