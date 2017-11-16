using SoftFX.Net.BotAgent;
using System;

namespace TickTrader.Algo.Protocol
{
    public class AccountListRequestEntity
    {
        public string Id { get; internal set; }


        public AccountListRequestEntity()
        {
            Id = Guid.NewGuid().ToString();
        }

        internal AccountListRequestEntity(AccountListRequest request)
        {
            Id = request.Id;
        }
    }


    internal static class AccountListRequestEntityExtensions
    {
        internal static AccountListRequestEntity ToEntity(this AccountListRequest request)
        {
            var res = new AccountListRequestEntity { Id = request.Id };
            return res;
        }

        internal static AccountListRequest ToMessage(this AccountListRequestEntity request)
        {
            var res = new AccountListRequest(0) { Id = request.Id };
            return res;
        }
    }
}
