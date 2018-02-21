using SoftFX.Net.BotAgent;

namespace TickTrader.Algo.Protocol
{
    public enum ConnectionErrorCode
    {
        None,
        Unknown,
        NetworkError,
        Timeout,
        BlockedAccount,
        ClientInitiated,
        InvalidCredentials,
        SlowConnection,
        ServerError,
        LoginDeleted,
        ServerLogout,
        Canceled,
    }


    public class ConnectionErrorEntity
    {
        public ConnectionErrorCode Code { get; set; }

        public string Text { get; set; }


        internal void UpdateModel(ConnectionErrorModel model)
        {
            model.Code = ToSfx.Convert(Code);
            model.Text = Text;
        }

        internal void UpdateSelf(ConnectionErrorModel model)
        {
            Code = ToAlgo.Convert(model.Code);
            Text = model.Text;
        }
    }
}
