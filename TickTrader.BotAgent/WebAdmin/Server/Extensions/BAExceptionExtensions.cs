using TickTrader.BotAgent.BA.Exceptions;
using TickTrader.BotAgent.WebAdmin.Server.Models;

namespace TickTrader.BotAgent.WebAdmin.Server.Extensions
{
    public static class BAExceptionExtensions
    {
        public static BadRequestResult ToBadResult(this BAException dsex)
        {
            return new BadRequestResult(dsex.Code, dsex.Message);
        }
    }
}
