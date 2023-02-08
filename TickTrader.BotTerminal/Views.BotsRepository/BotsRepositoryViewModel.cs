using Caliburn.Micro;
using Machinarium.ObservableCollections;
using Machinarium.Var;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using TickTrader.FDK.Calculator;

namespace TickTrader.BotTerminal.Views.BotsRepository
{
    internal sealed class BotsRepositoryViewModel : Screen, IWindowModel
    {
        private readonly Dictionary<string, BotInfoViewModel> _botsInfo = new();

        private readonly VarContext _context = new();
        private readonly IAlgoAgent _agent;

        private BoolVar _hasSelectedBots = new();


        public ObservableRangeCollection<BotInfoViewModel> CurrentBots { get; } = new();

        public ObservableCollection<SourceViewModel> Sources { get; } = new()
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

        public Property<BotInfoViewModel> SelectedBot { get; }

        public IntProperty SelectedTabIndex { get; }

        public BoolProperty OnlyWithUpdates { get; }

        public BoolProperty SelectAllBots { get; }

        public BoolProperty CanUpdateBots { get; }

        public string Name { get; }


        public BotsRepositoryViewModel(IAlgoAgent agent)
        {
            DisplayName = "Bots repository";
            _agent = agent;

            SelectedBot = _context.AddProperty<BotInfoViewModel>();
            SelectedBot.Value = null;

            SelectAllBots = _context.AddBoolProperty().AddPostTrigger(SetSelectAllBots);
            OnlyWithUpdates = _context.AddBoolProperty().AddPostTrigger(SetUpdateFilter);
            SelectedTabIndex = _context.AddIntProperty().AddPostTrigger(ResetState);
            CanUpdateBots = _context.AddBoolProperty();
            Name = _agent.Name;

            LoadCurrentBots();

            _ = RefreshCollection();
        }


        public void UpdateAllSelectedBots()
        {

        }

        public void ResetState(int _)
        {
            SelectedBot.Value = null;
        }

        public async Task RefreshCollection()
        {
            await Task.WhenAll(Sources.Select(s => s.RefreshBotsInfo()));

            foreach (var source in Sources)
            {
                foreach (var remoteBote in source.BotsInfo)
                    if (_botsInfo.TryGetValue(remoteBote.Name, out var localBot))
                        localBot.SetRemoteBot(remoteBote);
            }
        }

        private void LoadCurrentBots()
        {
            foreach (var plugin in _agent.Plugins.Snapshot.Values)
            {
                if (plugin.Descriptor_.IsIndicator)
                    continue;


                if (!_botsInfo.TryGetValue(plugin.Descriptor_.DisplayName, out var botInfo))
                {
                    var botName = plugin.Descriptor_.DisplayName;

                    botInfo = new(botName);

                    _hasSelectedBots |= botInfo.IsSelected.Var;
                    _botsInfo.Add(botName, botInfo);
                }

                if (_agent.Packages.Snapshot.TryGetValue(plugin.Key.PackageId, out var package))
                    botInfo.ApplyPackage(plugin, package.Identity);
            }

            _hasSelectedBots.PropertyChanged += (s, e) => CanUpdateBots.Value = _hasSelectedBots.Value;

            SetUpdateFilter(false);
        }

        private void SetUpdateFilter(bool onlyWithUpdates)
        {
            CurrentBots.Clear();

            var filteredBots = _botsInfo.Values.Where(b => !onlyWithUpdates || b.CanUpload.Value)
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