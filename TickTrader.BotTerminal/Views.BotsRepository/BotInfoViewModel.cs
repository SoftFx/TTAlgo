using Machinarium.Var;
using System.Collections.ObjectModel;
using System.Windows;
using TickTrader.Algo.Domain;

namespace TickTrader.BotTerminal.Views.BotsRepository
{
    internal sealed class BotInfoViewModel
    {
        private readonly VarContext _context = new();
        private readonly bool _isLocal;


        public ObservableCollection<BotVersionViewModel> Versions { get; } = new();

        public string Name { get; }

        public StrProperty Description { get; }

        public StrProperty Category { get; }

        public StrProperty Version { get; }

        public StrProperty ApiVersion { get; }

        public StrProperty RemoteVersion { get; }

        public StrProperty PackageSize { get; }


        public BoolProperty CanUpload { get; }

        public BoolProperty IsSelected { get; }


        public Visibility IsLocal => _isLocal ? Visibility.Visible : Visibility.Collapsed;

        public Visibility IsRemote => _isLocal ? Visibility.Collapsed : Visibility.Visible;


        public BotInfoViewModel(string name, bool isLocal = false)
        {
            _isLocal = isLocal;

            Name = name;

            Description = _context.AddStrProperty();
            Category = _context.AddStrProperty();

            Version = _context.AddStrProperty();
            ApiVersion = _context.AddStrProperty();
            RemoteVersion = _context.AddStrProperty();

            PackageSize = _context.AddStrProperty();

            CanUpload = _context.AddBoolProperty();
            IsSelected = _context.AddBoolProperty();
        }


        public void ApplyPackage(PluginInfo plugin, PackageIdentity identity = null)
        {
            var descriptor = plugin.Descriptor_;

            Versions.Add(new BotVersionViewModel(plugin.Key.PackageId, descriptor, identity));

            if (!_isLocal)
            {
                CanUpload.Value = true;
                RemoteVersion.Value = descriptor.Version;
            }

            if (!Version.HasValue || IsBetterVersion(descriptor.Version))
            {
                Version.Value = descriptor.Version;
                ApiVersion.Value = descriptor.ApiVersionStr;
                Description.Value = descriptor.Description;
                Category.Value = descriptor.Category;

                //PackageSize.Value = $"{identity?.Size / 1024} KB";
            }
        }

        public void SetRemoteBot(BotInfoViewModel remoteBot)
        {
            var remoteVersion = remoteBot.Version.Value;

            RemoteVersion.Value = remoteVersion;
            CanUpload.Value = IsBetterVersion(RemoteVersion.Value);
        }

        private bool IsBetterVersion(string newVersion)
        {
            return string.Compare(newVersion, Version.Value) == 1;
        }
    }
}
