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


    public abstract class UpdateEntity<T> : UpdateEntity where T : new()
    {
        public T Item { get; set; }


        public UpdateEntity() : base()
        {
            Item = new T();
        }
    }


    internal static class UpdateEntityExtensions
    {
        internal static UpdateEntity ToEntity(this Update update)
        {
            return new UpdateEntity { Id = update.Id, Type = ToAlgo.Convert(update.Type) };
        }

        internal static Update ToMessage(this UpdateEntity update)
        {
            return new Update(0) { Id = update.Id, Type = ToSfx.Convert(update.Type) };
        }
    }
}
