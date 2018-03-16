using Caliburn.Micro;
using Machinarium.Qnil;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TickTrader.BotTerminal
{
    internal class ChartCollectionViewModel : Conductor<ChartViewModel>.Collection.OneActive
    {
        private Logger _logger;
        private TraderClientModel _clientModel;
        private IShell _shell;
        private readonly AlgoEnvironment _algoEnv;
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


        public ChartCollectionViewModel(TraderClientModel clientModel, IShell shell, AlgoEnvironment algoEnv, BotManagerViewModel botManager)
        {
            _logger = NLog.LogManager.GetCurrentClassLogger();
            _clientModel = clientModel;
            _algoEnv = algoEnv;
            _shell = shell;
            _botManager = botManager;

            clientModel.Symbols.Updated += Symbols_Updated;
        }


        public override void ActivateItem(ChartViewModel item)
        {
            base.ActivateItem(item);
            NotifyOfPropertyChange(nameof(SelectedChartProxy));
        }


        public void Open(string symbol, ChartPeriods period = ChartPeriods.M1)
        {
            ActivateItem(new ChartViewModel(symbol, period, _shell, _clientModel, _algoEnv, _botManager));
        }

        public void OpenOrActivate(string symbol, ChartPeriods period)
        {
            var chart = Items.FirstOrDefault(c => c.Symbol == symbol && c.SelectedPeriod.Key == period);
            if (chart != null)
            {
                ActivateItem(chart);
                return;
            }
            Open(symbol, period);
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

        public void SaveChartsSnapshot(ProfileStorageModel profileStorage)
        {
            try
            {
                profileStorage.SelectedChart = (SelectedChartProxy as ChartViewModel)?.Symbol;
                profileStorage.Charts = new List<ChartStorageEntry>();
                Items.Foreach(i => profileStorage.Charts.Add(i.GetSnapshot()));
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to save charts snapshot");
            }
        }

        public void LoadChartsSnaphot(ProfileStorageModel profileStorage, CancellationToken token)
        {
            try
            {
                if ((profileStorage.Charts?.Count ?? 0) == 0)
                {
                    _logger.Info($"Charts snapshot is empty");
                    return;
                }

                _logger.Info($"Loading charts snapshot({profileStorage.Charts.Count} bots)");

                foreach (var chart in profileStorage.Charts.Where(c => _clientModel.Symbols.GetOrDefault(c.Symbol) != null))
                {
                    if (token.IsCancellationRequested)
                    {
                        return;
                    }
                    var item = new ChartViewModel(chart.Symbol, chart.SelectedPeriod, _shell, _clientModel, _algoEnv, _botManager);
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
                _logger.Error(ex, "Failed to load charts snapshot");
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
