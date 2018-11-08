using SoftFX.Net.BotAgent;

namespace TickTrader.Algo.Protocol.Sfx
{
    public class PackageListRequestEntity : RequestEntity
    {
    }


    internal static class PackageListRequestEntityExtensions
    {
        internal static PackageListRequestEntity ToEntity(this PackageListRequest request)
        {
            return new PackageListRequestEntity { Id = request.Id };
        }

        internal static PackageListRequest ToMessage(this PackageListRequestEntity request)
        {
            return new PackageListRequest(0) { Id = request.Id };
        }
    }
}
