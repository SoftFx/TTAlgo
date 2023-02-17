using Caliburn.Micro;
using Machinarium.ObservableCollections;
using Machinarium.Var;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

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
                Link = "https://github.com/AndrewKhloptsau/AlgoBots",
            },

            new() //for test
            {
                Name = "Empty Source",
            }
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

        public BoolProperty CanUpdateBots { get; }

        public BoolProperty CollectionsRefreshed { get; }

        public string Name { get; }


        public BotsRepositoryViewModel(IAlgoAgent agent)
        {
            DisplayName = "Bots repository";
            _agent = agent;

            SelectedBot = _context.AddProperty<BotInfoViewModel>();
            SelectedBot.Value = null;

            SelectAllBots = _context.AddBoolProperty().AddPostTrigger(SetSelectAllBots);
            OnlyWithUpdates = _context.AddBoolProperty().AddPostTrigger(UpdateInstalledView);
            SelectedTabIndex = _context.AddIntProperty().AddPostTrigger(ResetState);
            CanUpdateBots = _context.AddBoolProperty();
            CollectionsRefreshed = _context.AddBoolProperty();

            FilteredInstalledString = _context.AddStrProperty().AddPostTrigger(FilterInstalledBotsByName);
            FilteredSourceString = _context.AddStrProperty().AddPostTrigger(FilterSourceBotsByName);
            Name = _agent.Name;

            LoadInstalledBots();

            _ = RefreshSources();

            _hasSelectedBots.PropertyChanged += (s, e) => CanUpdateBots.Value = _hasSelectedBots.Value;
        }


        public void UpdateAllSelectedBots()
        {

        }

        public void ResetState(int _)
        {
            SelectedBot.Value = null;
        }

        public Task RefreshSources() =>
            UpdateCollectionView(Sources, async () =>
            {
                await Task.WhenAll(_sources.Select(s => s.RefreshBotsInfo()));

                foreach (var source in _sources)
                {
                    foreach (var remoteBote in source.BotsInfo)
                        if (_botsInfo.TryGetValue(remoteBote.Name, out var localBot))
                            localBot.SetRemoteBot(remoteBote);

                    Sources.Add(source);
                }

                FilteredSourceString.Value = string.Empty;
            });

        public void FilterSourceBotsByName(string filter) =>
            _ = UpdateCollectionView(Sources, () =>
            {
                foreach (var source in _sources)
                    if (source.UpdateVisibleBotsCollection(filter))
                        Sources.Add(source);

                return Task.CompletedTask;
            });

        public void FilterInstalledBotsByName(string filter) =>
            _ = UpdateCollectionView(CurrentBots, () =>
            {
                foreach (var botInfo in _botsInfo.Values)
                    if (botInfo.IsVisibleBot(filter))
                        CurrentBots.Add(botInfo);

                return Task.CompletedTask;
            });

        private async Task UpdateCollectionView<T>(ObservableCollection<T> collection, Func<Task> update)
        {
            CollectionsRefreshed.Value = false;

            collection.Clear();
            SelectedBot.Value = null;

            await update();

            CollectionsRefreshed.Value = true;
        }

        private void LoadInstalledBots()
        {
            var instPlugins = _agent.Plugins.Snapshot.Values.Where(p => p.Descriptor_.IsTradeBot);
            var instPackages = _agent.Packages.Snapshot;

            foreach (var plugin in instPlugins)
            {
                var botName = plugin.Descriptor_.DisplayName;

                if (!_botsInfo.TryGetValue(botName, out var botInfo))
                {
                    botInfo = new(botName);

                    _hasSelectedBots |= botInfo.IsSelected.Var;
                    _botsInfo.Add(botName, botInfo);
                }

                if (instPackages.TryGetValue(plugin.Key.PackageId, out var package))
                    botInfo.ApplyPackage(plugin, package.Identity);
            }

            UpdateInstalledView(false);
        }

        private void UpdateInstalledView(bool useCanUpload)
        {
            CurrentBots.Clear();

            var filteredBots = _botsInfo.Values.Where(b => !useCanUpload || b.CanUpload.Value);

            CurrentBots.AddRange(filteredBots.OrderBy(b => b.Name));

            SetSelectAllBots(SelectAllBots.Value);
        }

        private void SetSelectAllBots(bool select)
        {
            foreach (var bot in CurrentBots)
                bot.IsSelected.Value = select;
        }
    }
}