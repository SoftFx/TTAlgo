
using TickTrader.DedicatedServer.DS.Models;

namespace TickTrader.DedicatedServer.DS
{
    public interface IDedicatedServer
    {
        PackageModel AddPackage(byte[] fileContent, string fileName);
        PackageModel[] GetPackages();
        void RemovePackage(string package);
    }
}
