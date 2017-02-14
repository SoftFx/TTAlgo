using System.Threading.Tasks;
using TickTrader.DedicatedServer.Server.Models;

namespace TickTrader.DedicatedServer.Server.DS
{
    public interface IDedicatedServer
    {
        Task AddPackageAsync(byte[] fileContent, string fileName);
        Task<PackageModel[]> GetPackagesAsync();
        Task RemovePackageAsync(string package);
    }
}
