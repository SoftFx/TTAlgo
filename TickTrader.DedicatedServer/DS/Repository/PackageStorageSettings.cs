namespace TickTrader.DedicatedServer.DS.Repository
{
    public class PackageStorageSettings
    {
        public PackageStorageSettings()
        {
            Path = "AlgoRepository";
        }

        public string Path { get; set; }
    }
}
