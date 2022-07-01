using System;
using System.Reflection;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Package
{
    public interface IPackageExplorer
    {
        PackageInfo ScanAssembly(string packageId, Assembly assembly, bool cacheResult);
    }


    public static class PackageExplorer
    {
        private static IPackageExplorer _explorer;
        private static string _assemblyFullName, _typeFullName;


        public static void Init<T>()
            where T : IPackageExplorer, new()
        {
            var type = typeof(T);
            _assemblyFullName = type.Assembly.FullName;
            _typeFullName = type.FullName;

            _explorer = new T();
        }

        public static void Init(IPackageExplorer explorer) => _explorer = explorer;

        public static ValueTuple<string, string> GetTypeInfo() => new ValueTuple<string, string>(_assemblyFullName, _typeFullName);


        public static PackageInfo ScanAssembly(string packageId, Assembly assembly, bool cacheResult = true)
        {
            return _explorer.ScanAssembly(packageId, assembly, cacheResult);
        }
    }
}
