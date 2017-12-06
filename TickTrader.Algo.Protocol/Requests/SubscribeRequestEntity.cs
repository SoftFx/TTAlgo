using SoftFX.Net.BotAgent;

namespace TickTrader.Algo.Protocol
{
    public class SubscribeRequestEntity : RequestEntity
    {
    }


    internal static class SubscribeRequestEntityExtensions
    {
        internal static SubscribeRequestEntity ToEntity(this SubscribeRequest request)
        {
            return new SubscribeRequestEntity { Id = request.Id };
        }

        internal static SubscribeRequest ToMessage(this SubscribeRequestEntity request)
        {
            return new SubscribeRequest(0) { Id = request.Id };
        }
    }
}
