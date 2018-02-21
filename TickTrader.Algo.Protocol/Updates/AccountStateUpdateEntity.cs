using SoftFX.Net.BotAgent;

namespace TickTrader.Algo.Protocol
{
    public class AccountStateUpdateEntity : UpdateEntity
    {
        public AccountKeyEntity Account { get; set; }

        public ConnectionState ConnectionState { get; set; }

        public ConnectionErrorEntity LastError { get; set; }


        public AccountStateUpdateEntity() : base()
        {
            Type = UpdateType.Updated;
            Account = new AccountKeyEntity();
            LastError = new ConnectionErrorEntity();
        }
    }


    internal static class AccountStateUpdateEntityExtensions
    {
        internal static AccountStateUpdateEntity ToEntity(this AccountStateUpdate update)
        {
            var res = new AccountStateUpdateEntity { Id = update.Id, Type = ToAlgo.Convert(update.Type), ConnectionState = ToAlgo.Convert(update.ConnectionState) };
            res.Account.UpdateSelf(update.Account);
            res.LastError.UpdateSelf(update.LastError);
            return res;
        }

        internal static AccountStateUpdate ToMessage(this AccountStateUpdateEntity update)
        {
            var res = new AccountStateUpdate(0) { Id = update.Id, Type = ToSfx.Convert(update.Type), ConnectionState = ToSfx.Convert(update.ConnectionState) };
            update.Account.UpdateModel(res.Account);
            update.LastError.UpdateModel(res.LastError);
            return res;
        }
    }
}
