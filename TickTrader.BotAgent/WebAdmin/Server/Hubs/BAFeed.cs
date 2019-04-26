using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using TickTrader.BotAgent.WebAdmin.Server.Dto;

namespace TickTrader.BotAgent.WebAdmin.Server.Hubs
{
    [Authorize]
    public class BAFeed : Hub<IBAFeed>
    {
    }

    public interface IBAFeed
    {
        Task AddOrUpdatePackage(PackageDto package);
        Task DeletePackage(string name);

        Task AddAccount(AccountDto account);
        Task DeleteAccount(AccountDto account);

        Task AddBot(TradeBotDto bot);
        Task DeleteBot(string botId);
        Task UpdateBot(TradeBotDto bot);

        Task ChangeBotState(BotStateDto status);
    }
}
