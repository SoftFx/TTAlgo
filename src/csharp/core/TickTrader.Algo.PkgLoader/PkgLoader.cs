using TickTrader.Algo.CoreV1;
using TickTrader.Algo.Package;

namespace TickTrader.Algo.PkgLoader
{
    public static class PkgLoader
    {
        public static void InitDefaults()
        {
            PackageLoadContext.Init(Isolation.PackageLoadContextProvider.Create);
            PackageExplorer.Init<PackageV1Explorer>();
        }
    }
}