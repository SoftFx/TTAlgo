using SoftFX.Net.BotAgent;

namespace TickTrader.Algo.Protocol
{
    public class AccountListRequestEntity : RequestEntity
    {
    }


    internal static class AccountListRequestEntityExtensions
    {
        internal static AccountListRequestEntity ToEntity(this AccountListRequest request)
        {
            return new AccountListRequestEntity { Id = request.Id };
        }

        internal static AccountListRequest ToMessage(this AccountListRequestEntity request)
        {
            return new AccountListRequest(0) { Id = request.Id };
        }
    }
}
