using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using TickTrader.DedicatedServer.WebAdmin.Server.Dto;

namespace TickTrader.DedicatedServer.WebAdmin.Server.Hubs
{
    public class DSFeed : Hub<IDSFeed>
    {
    }

    public interface IDSFeed
    {
        Task AddPackage(PackageDto package);
        Task DeletePackage(string name);

        Task AddAccount(AccountDto account);
        Task DeleteAccount(AccountDto account);
    }
}
