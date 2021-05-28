using Caliburn.Micro;
using Machinarium.Qnil;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Domain;

namespace TickTrader.BotTerminal
{
    internal class ChartCollectionViewModel : Conductor<ChartViewModel>.Collection.OneActive
    {
        private Logger _logger;
        private TraderClientModel _clientModel;
        private IShell _shell;
        private readonly AlgoEnvironment _algoEnv;
        private Dictionary<string, ChartViewModel> _charts;


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


        public ChartCollectionViewModel(TraderClientModel clientModel, IShell shell, AlgoEnvironment algoEnv)
        {
            _logger = NLog.LogManager.GetCurrentClassLogger();
            _clientModel = clientModel;
            _algoEnv = algoEnv;
            _shell = shell;
            _charts = new Dictionary<string, ChartViewModel>();

            clientModel.Symbols.Updated += Symbols_Updated;
        }


        public override Task ActivateItemAsync(ChartViewModel item, CancellationToken cancellationToken = default)
        {
            _charts[item.ChartWindowId] = item;
            NotifyOfPropertyChange(nameof(SelectedChartProxy));
            return base.ActivateItemAsync(item, cancellationToken);
        }

        //public override void ActivateItem(ChartViewModel item)
        //{
        //    base.ActivateItem(item);
        //    _charts[item.ChartWindowId] = item;
        //    NotifyOfPropertyChange(nameof(SelectedChartProxy));
        //}

        public void Open(string symbol, ChartPeriods period = ChartPeriods.M1)
        {
            ActivateItemAsync(new ChartViewModel(GenerateChartId(), symbol, period, _algoEnv));
        }

        public void OpenOrActivate(string symbol, ChartPeriods period)
        {
            var chart = Items.FirstOrDefault(c => c.Symbol == symbol && c.SelectedPeriod.Key == period);
            if (chart != null)
            {
                ActivateItemAsync(chart);
                return;
            }
            Open(symbol, period);
        }

        public void CloseItem(ChartViewModel chart)
        {
            chart.TryCloseAsync();
            _charts.Remove(chart.ChartWindowId);
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
                Items.ForEach(i => profileStorage.Charts.Add(i.GetSnapshot()));
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

                _logger.Info($"Loading charts snapshot({profileStorage.Charts.Count} charts)");

                _charts.Clear();

                profileStorage.Charts.Where(c => c.Id != null).ForEach(c => _charts[c.Id] = null); // register existing chartIds

                foreach (var chart in profileStorage.Charts.Where(c => _clientModel.Symbols.GetOrDefault(c.Symbol) != null))
                {
                    if (token.IsCancellationRequested)
                    {
                        return;
                    }

                    var id = chart.Id ?? GenerateChartId(); // generate missing chartIds
                    var item = new ChartViewModel(id, chart.Symbol, chart.SelectedPeriod, _algoEnv);
                    ActivateItemAsync(item);
                    item.RestoreFromSnapshot(chart);
                }

                var selectedItem = Items.FirstOrDefault(c => c.Symbol == profileStorage.SelectedChart);
                if (selectedItem != null)
                {
                    ActivateItemAsync(selectedItem);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to load charts snapshot");
            }
        }


        private void Symbols_Updated(DictionaryUpdateArgs<string, SymbolInfo> args)
        {
            if (args.Action == DLinqAction.Remove)
            {
                var toRemove = Items.Where(i => i.Symbol == args.OldItem.Name).ToList();

                foreach (var chart in toRemove)
                    CloseItem(chart);
            }
        }

        private string GenerateChartId()
        {
            var i = 0;
            while (true)
            {
                var idCandidate = $"Chart{i}";
                if (!_charts.ContainsKey(idCandidate))
                {
                    return idCandidate;
                }
                i++;
            }
        }
    }
}
