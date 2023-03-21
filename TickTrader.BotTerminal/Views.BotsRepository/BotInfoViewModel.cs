﻿using Machinarium.ObservableCollections;
using Machinarium.Var;
using System;
using System.Linq;
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

        private Func<BotInfoViewModel, Task<string>> _downloadPackageHandler;
        private BotInfoViewModel _remoteVersion;


        public ObservableRangeCollection<BotVersionViewModel> Versions { get; } = new();

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

        public StrProperty ErrorMessage { get; }

        public StrProperty ResultMessage { get; }


        public BoolProperty CanUpload { get; }

        public BoolProperty IsSelected { get; }

        public BoolProperty IsUploading { get; }


        public Visibility IsLocal => _isLocal ? Visibility.Visible : Visibility.Collapsed;

        public Visibility IsRemote => _isLocal ? Visibility.Collapsed : Visibility.Visible;


        internal string PackageName { get; private set; }


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
            ErrorMessage = _context.AddStrProperty();
            ResultMessage = _context.AddStrProperty();

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

        public BotInfoViewModel(PluginsInfo remotePlugin, Func<BotInfoViewModel, Task<string>> download) : this()
        {
            _downloadPackageHandler = download;
            _isLocal = false;

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
                ErrorMessage.Value = null;
                ResultMessage.Value = null;

                IsUploading.Value = true;

                try
                {

                    if (_downloadPackageHandler != null)
                    {
                        var downloadedFileName = await _downloadPackageHandler.Invoke(_isLocal ? _remoteVersion : this);

                        ResultMessage.Value = $"Success. File has been stored in: {downloadedFileName}";
                    }
                }
                catch (Exception ex)
                {
                    ErrorMessage.Value = $"Error: {ex.Message}";
                }

                IsUploading.Value = false;
            }
        }


        internal BotInfoViewModel ApplyPackage(PluginInfo plugin, PackageInfoViewModel identity)
        {
            Versions.Add(new BotVersionViewModel(plugin.Descriptor_, identity));

            return FindBestVersion();
        }

        internal BotInfoViewModel ApplyPackage(MetadataInfo info)
        {
            PackageName = info.PackageName;

            CanUpload.Value = true;

            Source.Value = info.Source;
            Author.Value = info.Author;

            ApiVersion.Value = info.ApiVersion;
            BuildData.Value = info.BuildDate;
            PackageSize.Value = info.PackageSize.ToKB();

            return this;
        }


        internal void RemoveVersion(PluginInfo plugin)
        {
            var packageId = plugin.Key.PackageId;
            var detectedVersion = Versions.FirstOrDefault(u => u.PackageInfo.Id == packageId);

            if (detectedVersion != null)
                Versions.Remove(detectedVersion);

            FindBestVersion();
        }

        internal void SetRemoteBot(BotInfoViewModel remoteBot)
        {
            var newVersion = remoteBot.Version.Value;

            if (IsBetterVersion(newVersion))
            {
                _remoteVersion = remoteBot;
                _downloadPackageHandler = remoteBot._downloadPackageHandler;

                CanUpload.Value = true;
                RemoteVersion.Value = newVersion;
            }
        }

        internal void ResetNewVersion()
        {
            _remoteVersion = null;
            _downloadPackageHandler = null;

            CanUpload.Value = false;
            RemoteVersion.Value = null;
        }

        internal bool IsVisibleBot(string filter)
        {
            return Name.Contains(filter, StringComparison.OrdinalIgnoreCase) || string.IsNullOrEmpty(filter);
        }


        private BotInfoViewModel FindBestVersion()
        {
            Version.Value = null;

            SortVersions();

            foreach (var version in Versions)
            {
                var descriptor = version.Descriptior;

                if (!Version.HasValue || IsBetterVersion(descriptor.Version))
                {
                    Version.Value = descriptor.Version;
                    ApiVersion.Value = descriptor.ApiVersionStr;
                    Description.Value = descriptor.Description;
                    Category.Value = descriptor.Category;
                }
            }

            return this;
        }

        private void SortVersions()
        {
            var versions = Versions.OrderByDescending(u => u.Version).ToList();

            Versions.Clear();
            Versions.AddRange(versions);
        }

        private bool IsBetterVersion(string newVersion) => string.Compare(newVersion, Version.Value) == 1;
    }
}
