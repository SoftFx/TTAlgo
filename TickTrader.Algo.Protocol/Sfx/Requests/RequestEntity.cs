using SoftFX.Net.BotAgent;
using System;

namespace TickTrader.Algo.Protocol.Sfx
{
    public class RequestEntity
    {
        public string Id { get; set; }


        public RequestEntity()
        {
            Id = Guid.NewGuid().ToString();
        }
    }


    internal static class RequestEntityExtensions
    {
        internal static RequestEntity ToEntity(this Request request)
        {
            return new RequestEntity { Id = request.Id };
        }

        internal static Request ToMessage(this RequestEntity request)
        {
            return new Request(0) { Id = request.Id };
        }
    }
}
