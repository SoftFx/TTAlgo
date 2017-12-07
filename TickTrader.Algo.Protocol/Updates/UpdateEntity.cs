using SoftFX.Net.BotAgent;
using System;

namespace TickTrader.Algo.Protocol
{
    public enum UpdateType
    {
        Added,
        Updated,
        Removed,
    }


    public class UpdateEntity
    {
        public string Id { get; set; }

        public UpdateType Type { get; set; }


        public UpdateEntity()
        {
            Id = Guid.NewGuid().ToString();
        }
    }


    public abstract class UpdateEntity<T> : UpdateEntity
    {
        public T OldItem { get; set; }

        public T NewItem { get; set; }


        public UpdateEntity() : base()
        {
            OldItem = default(T);
            NewItem = default(T);
        }
    }


    internal static class UpdateEntityExtensions
    {
        internal static UpdateEntity ToEntity(this Update update)
        {
            return new UpdateEntity { Id = update.Id };
        }

        internal static Update ToMessage(this UpdateEntity update)
        {
            return new Update(0) { Id = update.Id };
        }
    }
}
