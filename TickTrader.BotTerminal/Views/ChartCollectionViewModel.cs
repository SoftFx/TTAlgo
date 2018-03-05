using Caliburn.Micro;
using Machinarium.Qnil;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TickTrader.BotTerminal
{
    internal class ChartCollectionViewModel : Conductor<ChartViewModel>.Collection.OneActive
    {
        private Logger _logger;
        private TraderClientModel _clientModel;
        private IShell _shell;
        private readonly AlgoEnvironment _algoEnv;
        private PersistModel _storage;
        private BotManagerViewModel _botManager;


        public object SelectedChartProxy
        {
            get { return this.ActiveItem; }
            set
            {
                var chart = value as ChartViewModel;
                if (chart != null)
                    this.ActiveItem = chart;
            }
        }


        public ChartCollectionViewModel(TraderClientModel clientModel, IShell shell, AlgoEnvironment algoEnv, PersistModel storage, BotManagerViewModel botManager)
        {
            _logger = NLog.LogManager.GetCurrentClassLogger();
            _clientModel = clientModel;
            _algoEnv = algoEnv;
            _shell = shell;
            _storage = storage;
            _botManager = botManager;

            clientModel.Symbols.Updated += Symbols_Updated;
        }


        public override void ActivateItem(ChartViewModel item)
        {
            base.ActivateItem(item);
            NotifyOfPropertyChange(nameof(SelectedChartProxy));
        }


        public void Open(string symbol)
        {
            ActivateItem(new ChartViewModel(symbol, _shell, _clientModel, _algoEnv, _storage, _botManager));
        }

        public void CloseItem(ChartViewModel chart)
        {
            chart.TryClose();
        }

        public void CloseAllItems(CancellationToken token)
        {
            while (Items.Count > 0)
            {
                if (token.IsCancellationRequested)
                {
                    return;
                }
                var first = Items.FirstOrDefault();
                if (first != null)
                {
                    CloseItem(first);
                }
            }
        }

        public void SaveProfileSnapshot(ProfileStorageModel profileStorage)
        {
            try
            {
                profileStorage.SelectedChart = (SelectedChartProxy as ChartViewModel)?.Symbol;
                profileStorage.Charts = new List<ChartStorageEntry>();
                Items.Foreach(i => profileStorage.Charts.Add(i.GetSnapshot()));
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to save profile snapshot");
            }
        }

        public void LoadProfileSnapshot(ProfileStorageModel profileStorage, CancellationToken token)
        {
            try
            {
                foreach (var chart in profileStorage.Charts.Where(c => _clientModel.Symbols.GetOrDefault(c.Symbol) != null))
                {
                    if (token.IsCancellationRequested)
                    {
                        return;
                    }
                    var item = new ChartViewModel(chart.Symbol, _shell, _clientModel, _algoEnv, _storage, _botManager);
                    ActivateItem(item);
                    item.RestoreFromSnapshot(chart);
                }

                var selectedItem = Items.FirstOrDefault(c => c.Symbol == profileStorage.SelectedChart);
                if (selectedItem != null)
                {
                    ActivateItem(selectedItem);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to load profile snapshot");
            }
        }


        private void Symbols_Updated(DictionaryUpdateArgs<string, Algo.Common.Model.SymbolModel> args)
        {
            if (args.Action == DLinqAction.Remove)
            {
                var toRemove = Items.Where(i => i.Symbol == args.OldItem.Name).ToList();

                foreach (var chart in toRemove)
                    CloseItem(chart);
            }
        }
    }
}
