using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using TickTrader.DedicatedServer.WebAdmin.Server.Dto;

namespace TickTrader.DedicatedServer.WebAdmin.Server.Hubs
{
    [Authorize]
    public class DSFeed : Hub<IDSFeed>
    {
    }

    public interface IDSFeed
    {
        Task AddPackage(PackageDto package);
        Task DeletePackage(string name);

        Task AddAccount(AccountDto account);
        Task DeleteAccount(AccountDto account);

        Task AddBot(TradeBotDto bot);
        Task DeleteBot(string botId);
        Task UpdateBot(TradeBotDto bot);

        Task ChangeBotState(BotStateDto status);
    }
}
