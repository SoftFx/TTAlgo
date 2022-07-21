using TickTrader.Algo.Package;

namespace TickTrader.Algo.Isolation
{
    public static class PackageLoadContextProvider
    {
        public static IPackageLoadContext Create(bool isolated)
        {
            return isolated ? new IsolatedLoadContext() : (IPackageLoadContext)new DefaultLoadContext();
        }
    }
}
