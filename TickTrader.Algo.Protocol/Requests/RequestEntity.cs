using SoftFX.Net.BotAgent;
using System;

namespace TickTrader.Algo.Protocol
{
    public class RequestEntity
    {
        public string Id { get; set; }


        public RequestEntity()
        {
            Id = Guid.NewGuid().ToString();
        }

        internal RequestEntity(Request request)
        {
            Id = request.Id;
        }
    }


    internal static class RequestEntityExtensions
    {
        internal static RequestEntity ToEntity(this Request request)
        {
            var res = new RequestEntity { Id = request.Id };
            return res;
        }

        internal static Request ToMessage(this RequestEntity request)
        {
            var res = new Request(0) { Id = request.Id };
            return res;
        }
    }
}
