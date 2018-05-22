using SoftFX.Net.BotAgent;

namespace TickTrader.Algo.Protocol.Sfx
{
    public enum ConnectionState
    {
        Offline,
        Connecting,
        Online,
        Disconnecting,
    }


    public class AccountModelEntity
    {
        public string Login { get; set; }

        public string Server { get; set; }

        public bool UseNewProtocol { get; set; }

        public ConnectionState ConnectionState { get; set; }

        public ConnectionErrorEntity LastError { get; set; }


        public AccountModelEntity()
        {
            LastError = new ConnectionErrorEntity();
        }


        internal void UpdateModel(AccountModel_1 model)
        {
            model.Login = Login;
            model.Server = Server;
        }

        internal void UpdateSelf(AccountModel_1 model)
        {
            Login = model.Login;
            Server = model.Server;
        }

        internal void UpdateModel(AccountModel model)
        {
            model.Login = Login;
            model.Server = Server;
            model.UseNewProtocol = UseNewProtocol;
            model.ConnectionState = ToSfx.Convert(ConnectionState);
            LastError.UpdateModel(model.LastError);
        }

        internal void UpdateSelf(AccountModel model)
        {
            Login = model.Login;
            Server = model.Server;
            UseNewProtocol = model.UseNewProtocol;
            ConnectionState = ToAlgo.Convert(model.ConnectionState);
            LastError.UpdateSelf(model.LastError);
        }
    }
}
