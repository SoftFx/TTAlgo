using TickTrader.DedicatedServer.DS.Models;

namespace TickTrader.DedicatedServer.DS
{
    public interface IPackageStorage
    {
        PackageModel Add(byte[] fileContent, string fileName);
        PackageModel[] GetAll();
        void Remove(string package);
    }
}
