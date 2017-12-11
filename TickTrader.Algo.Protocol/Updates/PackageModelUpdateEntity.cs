using SoftFX.Net.BotAgent;

namespace TickTrader.Algo.Protocol
{
    public class PackageModelUpdateEntity : UpdateEntity<PackageModelEntity>
    {
    }


    internal static class PackageModelUpdateEntityExtensions
    {
        internal static PackageModelUpdateEntity ToEntity(this PackageModelUpdate update)
        {
            var res = new PackageModelUpdateEntity { Id = update.Id, Type = ToAlgo.Convert(update.Type) };
            res.Item.UpdateSelf(update.Item);
            return res;
        }

        internal static PackageModelUpdate ToMessage(this PackageModelUpdateEntity update)
        {
            var res = new PackageModelUpdate(0) { Id = update.Id, Type = ToSfx.Convert(update.Type) };
            update.Item.UpdateModel(res.Item);
            return res;
        }
    }
}
