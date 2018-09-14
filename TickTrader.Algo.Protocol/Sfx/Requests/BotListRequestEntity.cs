using SoftFX.Net.BotAgent;

namespace TickTrader.Algo.Protocol.Sfx
{
    public class BotListRequestEntity : RequestEntity
    {
    }


    internal static class BotListRequestEntityExtensions
    {
        internal static BotListRequestEntity ToEntity(this BotListRequest request)
        {
            return new BotListRequestEntity { Id = request.Id };
        }

        internal static BotListRequest ToMessage(this BotListRequestEntity request)
        {
            return new BotListRequest(0) { Id = request.Id };
        }
    }
}
