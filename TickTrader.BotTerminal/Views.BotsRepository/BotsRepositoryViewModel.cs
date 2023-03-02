using Caliburn.Micro;
using Machinarium.ObservableCollections;
using Machinarium.Qnil;
using Machinarium.Var;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TickTrader.Algo.Domain;
using TickTrader.FDK.Calculator;

namespace TickTrader.BotTerminal.Views.BotsRepository
{
    internal sealed class BotsRepositoryViewModel : Screen, IWindowModel
    {
        private readonly Dictionary<string, BotInfoViewModel> _botsInfo = new();
        private readonly List<SourceViewModel> _sources = new()
        {
            new()
            {
                Name = "Public Bots",
                Link = "https://github.com/SoftFx/AlgoBots",
            },

            //new() //for test
            //{
            //    Name = "Empty Source",
            //}
        };

        private readonly VarContext _context = new();
        private readonly IAlgoAgent _agent;

        private BoolVar _hasSelectedBots = new();


        public ObservableRangeCollection<BotInfoViewModel> CurrentBots { get; } = new();

        public ObservableCollection<SourceViewModel> Sources { get; } = new();

        public Property<BotInfoViewModel> SelectedBot { get; }

        public StrProperty FilteredSourceString { get; }

        public StrProperty FilteredInstalledString { get; }

        public IntProperty SelectedTabIndex { get; }

        public BoolProperty OnlyWithUpdates { get; }

        public BoolProperty SelectAllBots { get; }

        public BoolProperty CanUpdateAllBots { get; }

        public BoolProperty CollectionsRefreshed { get; }

        public string Name { get; }


        public BotsRepositoryViewModel(IAlgoAgent agent)
        {
            _agent = agent;

            Name = _agent.Name;
            DisplayName = "Bots repository";

            SelectedBot = _context.AddProperty<BotInfoViewModel>();
            CanUpdateAllBots = _context.AddBoolProperty();
            CollectionsRefreshed = _context.AddBoolProperty();
            SelectAllBots = _context.AddBoolProperty().AddPostTrigger(SetSelectAllBots);
            OnlyWithUpdates = _context.AddBoolProperty().AddPostTrigger(_ => UpdateInstalledView());
            SelectedTabIndex = _context.AddIntProperty().AddPostTrigger(_ => SelectedBot.Reset());


            FilteredInstalledString = _context.AddStrProperty().AddPostTrigger(FilterInstalledBotsByName);
            FilteredSourceString = _context.AddStrProperty().AddPostTrigger(FilterSourceBotsByName);

            _hasSelectedBots.PropertyChanged += (s, e) => CanUpdateAllBots.Value = _hasSelectedBots.Value;
            _agent.Plugins.Updated += InstalledPluginsUpdatedHandler;
        }

        protected override Task OnActivateAsync(CancellationToken token)
        {
            SelectedBot.Reset();
            SelectAllBots.Reset();
            OnlyWithUpdates.Reset();

            FilteredInstalledString.Reset();
            FilteredSourceString.Reset();

            LoadInstalledBots();

            _ = RefreshSources(token); // to open window immediately without await 

            return Task.CompletedTask;
        }


        public async Task UpdateAllSelectedBots()
        {
            if (CanUpdateAllBots.Value)
            {
                CanUpdateAllBots.Value = false;

                var botsToUpdate = CurrentBots.Where(u => u.IsSelected.Value && u.CanUpload.Value);

                await Task.WhenAll(botsToUpdate.Select(u => u.DownloadPackage()));

                CanUpdateAllBots.Value = _hasSelectedBots.Value;
            }
        }

        public Task RefreshSources() => RefreshSources(CancellationToken.None); //for UI

        public void FilterSourceBotsByName(string filter) =>
            _ = UpdateCollectionView(Sources, () =>
            {
                foreach (var source in _sources)
                    if (source.UpdateVisibleBotsCollection(filter))
                        Sources.Add(source);

                return Task.CompletedTask;
            });

        public void FilterInstalledBotsByName(string filter) =>
            _ = UpdateInstalledView(filter);

        private Task RefreshSources(CancellationToken token) =>
            UpdateCollectionView(Sources, async () =>
            {
                await Task.WhenAll(_sources.Select(s => s.RefreshBotsInfo(token)));

                CheckNewVersionForInstalled();

                foreach (var source in _sources)
                    Sources.Add(source);

                FilteredSourceString.Value = string.Empty;
            });

        private async Task UpdateCollectionView<T>(ObservableCollection<T> collection, Func<Task> update)
        {
            CollectionsRefreshed.Value = false;

            collection.Clear();
            SelectedBot.Reset();

            await update();

            CollectionsRefreshed.Value = true;
        }

        private void LoadInstalledBots()
        {
            _botsInfo.Clear();

            var instPlugins = _agent.Plugins.Snapshot.Values.Where(p => p.Descriptor_.IsTradeBot);

            foreach (var plugin in instPlugins)
                RegisterNewPlugin(plugin);

            UpdateInstalledView();
        }

        private Task UpdateInstalledView(string filter = null) =>
            UpdateCollectionView(CurrentBots, () =>
            {
                var useCanUpload = OnlyWithUpdates?.Value ?? false;

                filter ??= FilteredInstalledString?.Value ?? string.Empty; //for property initialization

                var filteredBots = _botsInfo.Values
                    .Where(b => (!useCanUpload || b.CanUpload.Value) && b.IsVisibleBot(filter))
                    .OrderBy(b => b.Name);

                CurrentBots.AddRange(filteredBots);

                SetSelectAllBots(SelectAllBots.Value);

                return Task.CompletedTask;
            });

        private void SetSelectAllBots(bool select)
        {
            foreach (var bot in CurrentBots)
                bot.IsSelected.Value = select;
        }

        private void InstalledPluginsUpdatedHandler(DictionaryUpdateArgs<PluginKey, PluginInfo> args)
        {
            switch (args.Action)
            {
                case DLinqAction.Insert:
                    RegisterNewPlugin(args.NewItem);
                    break;
                case DLinqAction.Remove:
                    RemovePlugin(args.OldItem);
                    break;
                default:
                    break;
            }

            CheckNewVersionForInstalled();
            UpdateInstalledView();
        }

        private void RegisterNewPlugin(PluginInfo plugin)
        {
            if (!plugin.Descriptor_.IsTradeBot)
                return;

            var botName = plugin.Descriptor_.DisplayName;

            if (!_botsInfo.TryGetValue(botName, out var botInfo))
            {
                botInfo = new(botName);

                _hasSelectedBots |= botInfo.IsSelected.Var;
                _botsInfo.Add(botName, botInfo);
            }

            if (_agent.Packages.Snapshot.TryGetValue(plugin.Key.PackageId, out var package))
                botInfo.ApplyPackage(plugin, package.Identity);
        }

        private void RemovePlugin(PluginInfo plugin)
        {
            if (!plugin.Descriptor_.IsTradeBot)
                return;

            var botName = plugin.Descriptor_.DisplayName;

            if (_botsInfo.TryGetValue(botName, out var botInfo))
                botInfo.RemoveVersion(plugin);

            if (botInfo.Versions.Count == 0)
                _botsInfo.Remove(botName);
        }

        private void CheckNewVersionForInstalled()
        {
            _botsInfo.Values.Foreach(u => u.ResetNewVersion());

            foreach (var source in _sources)
            {
                foreach (var remoteBote in source.BotsInfo)
                    if (_botsInfo.TryGetValue(remoteBote.Name, out var localBot))
                        localBot.SetRemoteBot(remoteBote);
            }
        }
    }
}