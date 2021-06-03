using TickTrader.Algo.Domain;
using TickTrader.BotAgent.WebAdmin.Server.Models;

namespace TickTrader.BotAgent.WebAdmin.Server.Extensions
{
    public static class BAExceptionExtensions
    {
        public static BadRequestResultDto ToBadResult(this AlgoException algoEx)
        {
            return new BadRequestResultDto(0, algoEx.Message);
        }
    }
}
