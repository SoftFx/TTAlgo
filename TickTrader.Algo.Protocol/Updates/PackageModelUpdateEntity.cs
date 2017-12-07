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
            if (update.OldItem.HasValue)
            {
                res.OldItem = new PackageModelEntity();
                res.OldItem.UpdateSelf(update.OldItem.Value);
            }
            if (update.NewItem.HasValue)
            {
                res.NewItem = new PackageModelEntity();
                res.NewItem.UpdateSelf(update.NewItem.Value);
            }
            return res;
        }

        internal static PackageModelUpdate ToMessage(this PackageModelUpdateEntity update)
        {
            var res = new PackageModelUpdate(0) { Id = update.Id, Type = ToSfx.Convert(update.Type) };
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
