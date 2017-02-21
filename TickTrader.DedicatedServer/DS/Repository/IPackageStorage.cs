using TickTrader.DedicatedServer.DS.Models;

namespace TickTrader.DedicatedServer.DS.Repository
{
    public interface IPackageStorage
    {
        PackageModel Add(byte[] fileContent, string fileName);
        PackageModel[] GetAll();
        void Remove(string package);

    }
}
