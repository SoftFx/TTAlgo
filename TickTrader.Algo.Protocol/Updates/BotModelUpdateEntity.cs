using SoftFX.Net.BotAgent;

namespace TickTrader.Algo.Protocol
{
    public class BotModelUpdateEntity : UpdateEntity<BotModelEntity>
    {
    }


    internal static class BotModelUpdateEntityExtensions
    {
        internal static BotModelUpdateEntity ToEntity(this BotModelUpdate update)
        {
            var res = new BotModelUpdateEntity { Id = update.Id, Type = ToAlgo.Convert(update.Type) };
            if (update.OldItem.HasValue)
            {
                res.OldItem = new BotModelEntity();
                res.OldItem.UpdateSelf(update.OldItem.Value);
            }
            if (update.NewItem.HasValue)
            {
                res.NewItem = new BotModelEntity();
                res.NewItem.UpdateSelf(update.NewItem.Value);
            }
            return res;
        }

        internal static BotModelUpdate ToMessage(this BotModelUpdateEntity update)
        {
            var res = new BotModelUpdate(0) { Id = update.Id, Type = ToSfx.Convert(update.Type) };
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
