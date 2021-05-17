using System.Reflection;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Package
{
    public interface IPackageExplorer
    {
        PackageInfo ExamineAssembly(string packageId, Assembly assembly);
    }


    public static class PackageExplorer
    {
        private static IPackageExplorer _explorer;


        public static void Init(IPackageExplorer explorer)
        {
            _explorer = explorer;
        }


        public static PackageInfo ExamineAssembly(string packageId, Assembly assembly)
        {
            return _explorer.ExamineAssembly(packageId, assembly);
        }
    }
}
