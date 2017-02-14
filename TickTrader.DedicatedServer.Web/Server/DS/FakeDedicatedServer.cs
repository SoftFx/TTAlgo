using System.Threading.Tasks;
using TickTrader.DedicatedServer.Server.Models;

namespace TickTrader.DedicatedServer.Server.DS
{
    public class FakeDedicatedServer : IDedicatedServer
    {
        private IPackageStorage _packageStorage;
       
        public FakeDedicatedServer(IPackageStorage packageRepository)
        {
            _packageStorage = packageRepository;
        }

        public Task AddPackageAsync(byte[] fileContent, string fileName)
        {
            return _packageStorage.AddAsync(fileContent, fileName);
        }

        public Task<PackageModel[]> GetPackagesAsync()
        {
            return _packageStorage.GetAllAsync();
        }

        public Task RemovePackageAsync(string package)
        {
            return _packageStorage.RemoveAsync(package);
        }
    }
}
