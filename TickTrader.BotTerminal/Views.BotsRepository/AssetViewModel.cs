namespace TickTrader.BotTerminal.Views.BotsRepository
{
    internal sealed class AssetViewModel
    {
        public string Name { get; init; }

        public string Version { get; set; }


        public bool HasNewVersion { get; private set; }


        public void SetBetterVersion(string newVersion)
        {
            Version = $"{Version} -> {newVersion}";

            HasNewVersion = true;
        }
    }
}
