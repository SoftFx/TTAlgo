using SoftFX.Net.BotAgent;

namespace TickTrader.Algo.Protocol
{
    public class AccountModelUpdateEntity : UpdateEntity<AccountModelEntity>
    {
    }


    internal static class AccountModelUpdateEntityExtensions
    {
        internal static AccountModelUpdateEntity ToEntity(this AccountModelUpdate update)
        {
            var res = new AccountModelUpdateEntity { Id = update.Id, Type = ToAlgo.Convert(update.Type) };
            if (update.OldItem.HasValue)
            {
                res.OldItem = new AccountModelEntity();
                res.OldItem.UpdateSelf(update.OldItem.Value);
            }
            if (update.NewItem.HasValue)
            {
                res.NewItem = new AccountModelEntity();
                res.NewItem.UpdateSelf(update.NewItem.Value);
            }
            return res;
        }

        internal static AccountModelUpdate ToMessage(this AccountModelUpdateEntity update)
        {
            var res = new AccountModelUpdate(0) { Id = update.Id, Type = ToSfx.Convert(update.Type) };
            if (update.OldItem != null)
            {
                res.OldItem.New();
                update.OldItem.UpdateModel(res.OldItem.Value);
            }
            if (update.NewItem != null)
            {
                res.NewItem.New();
                update.NewItem.UpdateModel(res.NewItem.Value);
            }
            return res;
        }
    }
}
