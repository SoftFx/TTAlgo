using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using TickTrader.DedicatedServer.Server.Models;

namespace TickTrader.DedicatedServer.Server.DS
{
    public class FakePackageStorage : IPackageStorage
    {
        private IStorageWatcher _storageWatcher;
        private ConcurrentDictionary<string, PackageModel> _packages;

        public FakePackageStorage()
        {
            _packages = new ConcurrentDictionary<string, PackageModel>();
        }

        public FakePackageStorage(IStorageWatcher storageWatcher) : this()
        {
            _storageWatcher = storageWatcher;
        }

        public Task AddAsync(byte[] fileContent, string fileName)
        {
            return Task.Run(() => _packages.GetOrAdd(fileName, new PackageModel(fileName, new[] { new BotModel("Fake " + Guid.NewGuid()) })));
        }

        public Task<PackageModel[]> GetAllAsync()
        {
            return Task.FromResult(_packages.ToArray().Select(x => x.Value).ToArray());
        }

        public Task RemoveAsync(string packageName)
        {
            return Task.Run(() =>
            {
                PackageModel package;
                _packages.TryRemove(packageName, out package);
            });
        }
    }
}
