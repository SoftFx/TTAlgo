using TickTrader.Algo.Account;
using TickTrader.Algo.Account.Fdk2;

namespace TickTrader.Algo.Server
{
    public static class KnownAccountFactories
    {
        public static ServerInteropFactory Fdk2 => (options, loggerId) => new SfxInterop(options, loggerId);
    }
}
