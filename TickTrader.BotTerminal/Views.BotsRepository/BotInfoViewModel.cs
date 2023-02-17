using Machinarium.Var;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Tools.MetadataBuilder;

namespace TickTrader.BotTerminal.Views.BotsRepository
{
    internal sealed class BotInfoViewModel
    {
        private readonly VarContext _context = new();
        private readonly bool _isLocal;

        private Func<BotInfoViewModel, Task> _downloadPackageHandler;


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

        public BoolProperty IsUploading { get; }


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
            BuildData = _context.AddStrProperty();

            PackageSize = _context.AddStrProperty();

            CanUpload = _context.AddBoolProperty();
            IsSelected = _context.AddBoolProperty();
            IsUploading = _context.AddBoolProperty();
        }

        public BotInfoViewModel(string name) : this()
        {
            _isLocal = true;

            Name = name;
        }

        public BotInfoViewModel(PluginsInfo remotePlugin, Func<BotInfoViewModel, Task> download) : this()
        {
            _isLocal = false;
            _downloadPackageHandler = download;

            Name = remotePlugin.DisplayName;

            Version.Value = remotePlugin.Version;
            RemoteVersion.Value = remotePlugin.Version;
            Copyright.Value = remotePlugin.Copyright;
            Description.Value = remotePlugin.Description;
            Category.Value = remotePlugin.Category;
        }


        public void OpenSourceInBrowser() => SourceViewModel.OpenLinkInBrowser(Source.Value);

        public async Task DownloadPackage()
        {
            if (!IsUploading.Value)
            {
                IsUploading.Value = true;

                if (_downloadPackageHandler != null)
                    await _downloadPackageHandler.Invoke(this);

                IsUploading.Value = false;
            }
        }


        internal BotInfoViewModel ApplyPackage(PluginInfo plugin, PackageIdentity identity = null)
        {
            var descriptor = plugin.Descriptor_;

            Versions.Add(new BotVersionViewModel(plugin.Key.PackageId, descriptor, identity));

            if (!Version.HasValue || IsBetterVersion(descriptor.Version))
            {
                Version.Value = descriptor.Version;
                ApiVersion.Value = descriptor.ApiVersionStr;
                Description.Value = descriptor.Description;
                Category.Value = descriptor.Category;
            }

            return this;
        }

        internal BotInfoViewModel ApplyPackage(MetadataInfo info)
        {
            CanUpload.Value = true;

            Source.Value = info.Source;
            Author.Value = info.Author;

            ApiVersion.Value = info.ApiVersion;
            BuildData.Value = info.BuildDate;
            PackageSize.Value = $"{info.PackageSize / 1024} KB";

            return this;
        }

        internal void SetRemoteBot(BotInfoViewModel remoteBot)
        {
            var remoteVersion = remoteBot.Version.Value;

            RemoteVersion.Value = remoteVersion;
            CanUpload.Value = IsBetterVersion(RemoteVersion.Value);
        }

        internal bool IsVisibleBot(string filter)
        {
            return Name.Contains(filter, System.StringComparison.OrdinalIgnoreCase) || string.IsNullOrEmpty(filter);
        }

        private bool IsBetterVersion(string newVersion)
        {
            return string.Compare(newVersion, Version.Value) == 1;
        }
    }
}
