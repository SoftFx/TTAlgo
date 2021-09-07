using System.Reflection;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Package
{
    public interface IPackageExplorer
    {
        PackageInfo ScanAssembly(string packageId, Assembly assembly);
    }


    public static class PackageExplorer
    {
        private static IPackageExplorer _explorer;


        public static void Init(IPackageExplorer explorer)
        {
            _explorer = explorer;
        }


        public static PackageInfo ScanAssembly(string packageId, Assembly assembly)
        {
            return _explorer.ScanAssembly(packageId, assembly);
        }
    }
}
