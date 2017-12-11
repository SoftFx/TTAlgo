using SoftFX.Net.BotAgent;

namespace TickTrader.Algo.Protocol
{
    public class AccountModelEntity
    {
        public string Login { get; set; }

        public string Server { get; set; }


        internal void UpdateModel(AccountModel model)
        {
            model.Login = Login;
            model.Server = Server;
        }

        internal void UpdateSelf(AccountModel model)
        {
            Login = model.Login;
            Server = model.Server;
        }
    }
}
