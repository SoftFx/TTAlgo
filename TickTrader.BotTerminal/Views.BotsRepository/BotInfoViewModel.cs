using Machinarium.Var;
using System.Collections.ObjectModel;
using System.Security.Principal;
using System.Windows;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Tools.MetadataBuilder;

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

        public StrProperty Copyright { get; }

        public StrProperty Source { get; }

        public StrProperty Author { get; }

        public StrProperty BuildData { get; }

        public StrProperty Version { get; }

        public StrProperty ApiVersion { get; }

        public StrProperty RemoteVersion { get; }

        public StrProperty PackageSize { get; }


        public BoolProperty CanUpload { get; }

        public BoolProperty IsSelected { get; }


        public Visibility IsLocal => _isLocal ? Visibility.Visible : Visibility.Collapsed;

        public Visibility IsRemote => _isLocal ? Visibility.Collapsed : Visibility.Visible;


        private BotInfoViewModel()
        {
            Description = _context.AddStrProperty();
            Category = _context.AddStrProperty();
            Copyright = _context.AddStrProperty();
            Source = _context.AddStrProperty();
            Author = _context.AddStrProperty();

            Version = _context.AddStrProperty();
            ApiVersion = _context.AddStrProperty();
            RemoteVersion = _context.AddStrProperty();

            PackageSize = _context.AddStrProperty();

            CanUpload = _context.AddBoolProperty();
            IsSelected = _context.AddBoolProperty();
        }

        public BotInfoViewModel(string name) : this()
        {
            _isLocal = true;

            Name = name;
        }

        public BotInfoViewModel(PluginsInfo remotePlugin) : this()
        {
            _isLocal = false;

            Name = remotePlugin.DisplayName;

            Version.Value = remotePlugin.Version;
            Copyright.Value = remotePlugin.Copyright;
            Description.Value = remotePlugin.Description;
            Category.Value = remotePlugin.Category;
        }


        public BotInfoViewModel ApplyPackage(PluginInfo plugin, PackageIdentity identity = null)
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
            }
            
            return this;
        }

        public BotInfoViewModel ApplyPackage(MetadataInfo info)
        {
            Source.Value = info.Source;
            Author.Value = info.Author;

            ApiVersion.Value = info.ApiVersion;
            BuildData.Value = info.BuildDate;
            PackageSize.Value = $"{info.PackageSize / 1024} KB";

            return this;
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
