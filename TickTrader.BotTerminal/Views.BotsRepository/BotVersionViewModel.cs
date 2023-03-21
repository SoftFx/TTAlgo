using TickTrader.Algo.Domain;

namespace TickTrader.BotTerminal.Views.BotsRepository
{
    public sealed class BotVersionViewModel
    {
        public string Version { get; }

        public string ApiVersion { get; }

        public PackageInfoViewModel PackageInfo { get; }


        internal PluginDescriptor Descriptior { get; }


        public BotVersionViewModel(PluginDescriptor descriptor, PackageInfoViewModel package)
        {
            PackageInfo = package;
            Descriptior = descriptor;

            Version = descriptor.Version;
            ApiVersion = descriptor.ApiVersionStr;
        }
    }
}