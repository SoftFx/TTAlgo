using SoftFX.Net.BotAgent;

namespace TickTrader.Algo.Protocol.Sfx
{
    public class BotStateUpdateEntity : UpdateEntity
    {
        public string BotId { get; set; }

        public BotState State { get; set; }


        public BotStateUpdateEntity() : base()
        {
            Type = UpdateType.Updated;
        }
    }


    internal static class BotStateUpdateEntityExtensions
    {
        internal static BotStateUpdateEntity ToEntity(this BotStateUpdate update)
        {
            return new BotStateUpdateEntity { Id = update.Id, Type = ToAlgo.Convert(update.Type), BotId = update.BotId, State = ToAlgo.Convert(update.State) };
        }

        internal static BotStateUpdate ToMessage(this BotStateUpdateEntity update)
        {
            return new BotStateUpdate(0) { Id = update.Id, Type = ToSfx.Convert(update.Type), BotId = update.BotId, State = ToSfx.Convert(update.State) };
        }
    }
}
