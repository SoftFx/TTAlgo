using Caliburn.Micro;
using Machinarium.Qnil;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.BotTerminal
{
    internal class ChartCollectionViewModel : Conductor<ChartViewModel>.Collection.OneActive
    {
        private Logger _logger;
        private TraderClientModel _clientModel;
        private IShell _shell;
        private readonly AlgoEnvironment _algoEnv;


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
            this._clientModel = clientModel;
            this._algoEnv = algoEnv;
            this._shell = shell;

            clientModel.Symbols.Updated += Symbols_Updated;
        }


        public override void ActivateItem(ChartViewModel item)
        {
            base.ActivateItem(item);
            NotifyOfPropertyChange(nameof(SelectedChartProxy));
        }


        public void Open(string symbol)
        {
            ActivateItem(new ChartViewModel(symbol, _shell, _clientModel, _algoEnv));
        }

        public void CloseItem(ChartViewModel chart)
        {
            chart.TryClose();
        }

        public void CloseAllItems()
        {
            while (Items.Count > 0)
            {
                var first = Items.FirstOrDefault();
                if (first != null)
                {
                    CloseItem(first);
                }
            }
        }

        public void SaveProfile(ProfileStorageModel profileStorage)
        {
            try
            {
                profileStorage.SelectedChart = (SelectedChartProxy as ChartViewModel)?.Symbol;
                profileStorage.Charts = new List<ChartStorageEntry>();
                Items.Foreach(i =>
                {
                    profileStorage.Charts.Add(new ChartStorageEntry
                    {
                        Symbol = i.Symbol,
                        SelectedPeriod = i.SelectedPeriod.Key,
                        SelectedChartType = i.Chart.SelectedChartType,
                        CrosshairEnabled = i.Chart.IsCrosshairEnabled
                    });
                });
                profileStorage.Save();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to save current profile");
            }
        }

        public void LoadProfile(ProfileStorageModel profileStorage)
        {
            try
            {
                foreach (var chart in profileStorage.Charts.Where(c => _clientModel.Symbols.GetOrDefault(c.Symbol) != null))
                {
                    Open(chart.Symbol);
                    var item = SelectedChartProxy as ChartViewModel;
                    if (item != null)
                    {
                        item.SelectedPeriod = item.AvailablePeriods.FirstOrDefault(p => p.Key == chart.SelectedPeriod);
                        item.Chart.SelectedChartType = chart.SelectedChartType;
                        item.Chart.IsCrosshairEnabled = chart.CrosshairEnabled;
                    }
                }

                var selectedItem = Items.FirstOrDefault(c => c.Symbol == profileStorage.SelectedChart);
                if (selectedItem != null)
                {
                    ActivateItem(selectedItem);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to load current profile");
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
