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
            res.Item.UpdateSelf(update.Item);
            return res;
        }

        internal static BotModelUpdate ToMessage(this BotModelUpdateEntity update)
        {
            var res = new BotModelUpdate(0) { Id = update.Id, Type = ToSfx.Convert(update.Type) };
            update.Item.UpdateModel(res.Item);
            return res;
        }
    }
}
