using Caliburn.Micro;
using Machinarium.ObservableCollections;
using Machinarium.Var;
using Octokit;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TickTrader.Algo.Package;
using TickTrader.FDK.Calculator;

namespace TickTrader.BotTerminal.Views.BotsRepository
{
    internal sealed class BotsRepositoryViewModel : Screen, IWindowModel
    {
        private readonly Dictionary<string, BotMetainfoViewModel> _botsInfo = new();

        private readonly GitHubClient _client = new(new ProductHeaderValue("AndrewKhloptsau"));
        private readonly string _cacheFolder = Path.Combine(Environment.CurrentDirectory, "ReleaseCache");

        private readonly VarContext _context = new();
        private readonly IAlgoAgent _agent;

        private BoolVar _hasSelectedBots = new();


        public ObservableRangeCollection<BotMetainfoViewModel> CurrentBots { get; } = new();

        public ObservableCollection<ReleaseViewModel> Releases { get; } = new();

        public Property<BotMetainfoViewModel> SelectedBot { get; }

        public BoolProperty OnlyWithUpdates { get; }

        public BoolProperty SelectAllBots { get; }

        public BoolProperty CanUpdateBots { get; }

        public string Name { get; }


        public BotsRepositoryViewModel(IAlgoAgent agent)
        {
            DisplayName = "Bots repository";
            _agent = agent;

            SelectedBot = _context.AddProperty<BotMetainfoViewModel>();
            SelectedBot.Value = null;

            SelectAllBots = _context.AddBoolProperty().AddPostTrigger(SetSelectAllBots);
            OnlyWithUpdates = _context.AddBoolProperty().AddPostTrigger(SetUpdateFilter);
            CanUpdateBots = _context.AddBoolProperty();
            Name = _agent.Name;

            _ = RefreshCollection();
        }


        public void UpdateAllSelectedBots()
        {

        }


        public async Task RefreshCollection()
        {
            var release = await _client.Repository.Release.GetLatest("AndrewKhloptsau", "AlgoBots");

            Directory.CreateDirectory(_cacheFolder);

            var releaseFolder = Path.Combine(_cacheFolder, release.Name);

            if (!Directory.Exists(releaseFolder))
            {
                Directory.CreateDirectory(releaseFolder);

                foreach (var asset in release.Assets)
                {
                    var assetPath = Path.Combine(releaseFolder, asset.Name);

                    var response = await _client.Connection.Get<byte[]>(new Uri(asset.BrowserDownloadUrl), new Dictionary<string, string>(), "application/octet-stream");

                    using var fileStream = new FileStream(assetPath, System.IO.FileMode.Create, FileAccess.Write);

                    await fileStream.WriteAsync(response.Body.AsMemory(0, response.Body.Length));
                }
            }

            var releaseVm = new ReleaseViewModel
            {
                Name = release.Name,
            };

            foreach (var packagePath in Directory.GetFiles(releaseFolder))
            {
                var info = PackageLoadContext.ReflectionOnlyLoad(string.Empty, packagePath);

                foreach (var plugin in info.Plugins)
                {
                    var asset = new BotMetainfoViewModel(plugin.Descriptor_.DisplayName);
                    asset.ApplyPackage(plugin);
                    releaseVm.Plugins.Add(asset);
                }
            }

            Releases.Add(releaseVm);

            LoadCurrentBots();
        }

        private void LoadCurrentBots()
        {
            foreach (var plugin in _agent.Plugins.Snapshot.Values)
            {
                if (plugin.Descriptor_.IsIndicator)
                    continue;

                var botName = plugin.Descriptor_.DisplayName;
                var remote = Releases[0].Plugins.FirstOrDefault(p => p.Name == botName);

                if (!_botsInfo.TryGetValue(plugin.Descriptor_.DisplayName, out var botInfo))
                {
                    botInfo = new(botName);

                    _hasSelectedBots |= botInfo.IsSelected.Var;

                    _botsInfo.Add(botName, botInfo);
                }

                _agent.Packages.Snapshot.TryGetValue(plugin.Key.PackageId, out var package);

                botInfo.ApplyPackage(plugin, package?.Identity);

                if (remote is not null)
                    botInfo.SetRemoteVersion(remote.Version.Value);
            }

            _hasSelectedBots.PropertyChanged += (s, e) => CanUpdateBots.Value = _hasSelectedBots.Value;

            SetUpdateFilter(false);
        }

        private void SetUpdateFilter(bool onlyWithUpdates)
        {
            CurrentBots.Clear();

            var filteredBots = _botsInfo.Values.Where(b => !onlyWithUpdates || b.HasBetterVersion.Value)
                                               .OrderBy(b => b.Name);

            filteredBots.Foreach(u => u.IsSelected.Value = SelectAllBots.Value);

            CurrentBots.AddRange(filteredBots);
        }

        private void SetSelectAllBots(bool select)
        {
            foreach (var bot in CurrentBots)
                bot.IsSelected.Value = select;
        }
    }
}