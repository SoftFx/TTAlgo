using TickTrader.Algo.Account;

namespace TickTrader.Algo.Server
{
    public class AlgoServerSettings
    {
        public string DataFolder { get; set; }

        public ConnectionOptions ConnectionOptions { get; set; }

        public PkgStorageSettings PkgStorage { get; } = new PkgStorageSettings();
    }
}
