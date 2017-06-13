using TickTrader.DedicatedServer.DS.Exceptions;
using TickTrader.DedicatedServer.WebAdmin.Server.Models;

namespace TickTrader.DedicatedServer.WebAdmin.Server.Extensions
{
    public static class DSExceptionExtensions
    {
        public static BadRequestResult ToBadResult(this DSException dsex)
        {
            return new BadRequestResult(dsex.Code, dsex.Message);
        }
    }
}
