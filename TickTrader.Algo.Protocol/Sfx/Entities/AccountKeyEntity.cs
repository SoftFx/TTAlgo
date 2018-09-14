using SoftFX.Net.BotAgent;

namespace TickTrader.Algo.Protocol.Sfx
{
    public class AccountKeyEntity
    {
        public string Login { get; set; }

        public string Server { get; set; }


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
    }
}
