using TickTrader.Algo.Package;

namespace TickTrader.Algo.Isolation.NetCore
{
    public static class PackageLoadContextProvider
    {
        public static IPackageLoadContext Create(bool isolated)
        {
            //return new DefaultLoadContext();
            return isolated ? new IsolatedLoadContext() : (IPackageLoadContext)new DefaultLoadContext();
        }
    }
}
