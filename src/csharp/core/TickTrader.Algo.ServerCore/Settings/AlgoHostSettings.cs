using TickTrader.Algo.Package;

namespace TickTrader.Algo.Server
{
    public class AlgoHostSettings
    {
        public PkgStorageSettings PkgStorage { get; } = new PkgStorageSettings();

        public RuntimeSettings RuntimeSettings { get; } = new RuntimeSettings();
    }
}
