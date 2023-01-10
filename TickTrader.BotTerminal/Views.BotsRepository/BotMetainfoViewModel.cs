using Machinarium.Var;
using System.Collections.ObjectModel;
using TickTrader.Algo.Domain;

namespace TickTrader.BotTerminal.Views.BotsRepository
{
    internal sealed class BotMetainfoViewModel
    {
        private readonly VarContext _context = new();


        public ObservableCollection<BotVersionViewModel> Versions { get; } = new();


        public string Name { get; }

        public StrProperty Description { get; }

        public StrProperty Category { get; }


        public StrProperty Version { get; }

        public StrProperty RemoteVersion { get; }


        public BoolProperty HasBetterVersion { get; }

        public BoolProperty IsSelected { get; }


        public BotMetainfoViewModel(string name)
        {
            Name = name;

            Description = _context.AddStrProperty();
            Category = _context.AddStrProperty();

            Version = _context.AddStrProperty();
            RemoteVersion = _context.AddStrProperty();

            HasBetterVersion = _context.AddBoolProperty();
            IsSelected = _context.AddBoolProperty();
        }


        public void ApplyPackage(PluginInfo plugin, PackageIdentity identity = null)
        {
            var descriptor = plugin.Descriptor_;

            Versions.Add(new BotVersionViewModel(plugin.Key.PackageId, descriptor, identity));

            if (!Version.HasValue || IsBetterVersion(descriptor.Version))
            {
                Version.Value = descriptor.Version;
                Description.Value = descriptor.Description;
                Category.Value = descriptor.Category;
            }
        }

        public void SetRemoteVersion(string remoteVersion)
        {
            RemoteVersion.Value = remoteVersion;
            HasBetterVersion.Value = IsBetterVersion(RemoteVersion.Value);
        }

        private bool IsBetterVersion(string newVersion)
        {
            return string.Compare(newVersion, Version.Value) == 1;
        }
    }
}
