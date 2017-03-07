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
        Task DeletePackage(string name);
        Task AddPackage(PackageDto package);
    }
}
