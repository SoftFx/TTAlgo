using System.Threading.Tasks;
using TickTrader.DedicatedServer.Server.Models;

namespace TickTrader.DedicatedServer.Server.DS
{
    public interface IPackageStorage
    {
        Task AddAsync(byte[] fileContent, string fileName);
        Task<PackageModel[]> GetAllAsync();
        Task RemoveAsync(string package);
    }
}
