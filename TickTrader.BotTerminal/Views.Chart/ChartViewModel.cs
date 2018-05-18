using Caliburn.Micro;
using SciChart.Charting.ViewportManagers;
using SciChart.Charting.Visuals.RenderableSeries;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using TickTrader.Algo.Core.Repository;
using TickTrader.BotTerminal.Lib;
using SciChart.Charting.Visuals.Axes;
using SciChart.Data.Model;
using SciChart.Charting.Services;
using SciChart.Charting.Model.ChartSeries;
using SciChart.Charting.Model.DataSeries;
using NLog;
using Machinarium.Qnil;
using TickTrader.Algo.Api;
using TickTrader.Algo.Core.Metadata;
using System.Windows.Input;
using TickTrader.Algo.Common.Model;
using Xceed.Wpf.AvalonDock.Layout;
using System.Windows.Controls;
using TickTrader.Algo.Common.Info;

namespace TickTrader.BotTerminal
{
    public enum ChartPeriods { MN1, W1, D1, H4, H1, M30, M15, M5, M1, S10, S1, Ticks };


    class ChartViewModel : Screen, IDropHandler
    {
        private readonly Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private ChartModelBase activeChart;
        private readonly BarChartModel barChart;
        private readonly TickChartModel tickChart;
        private readonly IShell shell;
        private readonly VarList<ChartModelBase> charts = new VarList<ChartModelBase>();
        private readonly SymbolModel smb;
        private BotManagerViewModel _botManager;
        private IVarList<BotControlViewModel> _botsBySymbol;


        public ChartViewModel(string chartId, string symbol, ChartPeriods period, IShell shell, BotManagerViewModel botManager)
        {
            this.Symbol = symbol;
            this.DisplayName = symbol;
            this.shell = shell;
            _botManager = botManager;

            ChartWindowId = chartId;

            smb = shell.Agent.ClientModel.Symbols.GetOrDefault(symbol);

            Precision = smb.Descriptor.Precision;
            UpdateLabelFormat();

            this.barChart = new BarChartModel(smb, shell.Agent);
            this.tickChart = new TickChartModel(smb, shell.Agent);
            this.UiLock = new UiLock();

            var allIndicators = charts.SelectMany(c => c.Indicators);
            var dataSeries = charts.SelectMany(c => c.DataSeriesCollection);
            var indicatorViewModels = allIndicators.Chain().Select(i => new IndicatorViewModel(Chart, i, ChartWindowId, smb));
            var overlayIndicators = indicatorViewModels.Chain().Where(i => i.Model.HasOverlayOutputs);
            var overlaySeries = overlayIndicators.Chain().SelectMany(i => i.Series);
            var allSeries = Dynamic.CombineChained(dataSeries, overlaySeries);
            var paneIndicators = indicatorViewModels.Chain().Where(i => i.Model.HasPaneOutputs);
            var panes = paneIndicators.Chain().SelectMany(i => i.Panes);

            Series = allSeries.AsObservable();

            Indicators = indicatorViewModels.AsObservable();
            Panes = panes.AsObservable();
            _botsBySymbol = _botManager.Bots.Where(bc => bc.Model.Config.MainSymbol == Symbol);// && bc.Model.Setup.Descriptor.SetupMainSymbol);

            periodActivatos.Add(ChartPeriods.MN1, () => ActivateBarChart(TimeFrames.MN, "MMMM yyyy"));
            periodActivatos.Add(ChartPeriods.W1, () => ActivateBarChart(TimeFrames.W, "d MMMM yyyy"));
            periodActivatos.Add(ChartPeriods.D1, () => ActivateBarChart(TimeFrames.D, "d MMMM yyyy"));
            periodActivatos.Add(ChartPeriods.H4, () => ActivateBarChart(TimeFrames.H4, "d MMMM yyyy HH:mm"));
            periodActivatos.Add(ChartPeriods.H1, () => ActivateBarChart(TimeFrames.H1, "d MMMM yyyy HH:mm"));
            periodActivatos.Add(ChartPeriods.M30, () => ActivateBarChart(TimeFrames.M30, "d MMMM yyyy HH:mm"));
            periodActivatos.Add(ChartPeriods.M15, () => ActivateBarChart(TimeFrames.M15, "d MMMM yyyy HH:mm"));
            periodActivatos.Add(ChartPeriods.M5, () => ActivateBarChart(TimeFrames.M5, "d MMMM yyyy HH:mm"));
            periodActivatos.Add(ChartPeriods.M1, () => ActivateBarChart(TimeFrames.M1, "d MMMM yyyy HH:mm"));
            periodActivatos.Add(ChartPeriods.S10, () => ActivateBarChart(TimeFrames.S10, "d MMMM yyyy HH:mm:ss"));
            periodActivatos.Add(ChartPeriods.S1, () => ActivateBarChart(TimeFrames.S1, "d MMMM yyyy HH:mm:ss"));
            periodActivatos.Add(ChartPeriods.Ticks, () => ActivateTickChart());

            SelectedPeriod = periodActivatos.ContainsKey(period) ? periodActivatos.FirstOrDefault(p => p.Key == period) : periodActivatos.ElementAt(8);

            CloseCommand = new GenericCommand(o => TryClose());
        }

        #region Bindable Properties

        private readonly Dictionary<ChartPeriods, System.Action> periodActivatos = new Dictionary<ChartPeriods, System.Action>();
        private KeyValuePair<ChartPeriods, System.Action> selectedPeriod;

        public string ChartWindowId { get; }

        public Dictionary<ChartPeriods, System.Action> AvailablePeriods => periodActivatos;

        public ChartModelBase Chart
        {
            get { return activeChart; }
            private set
            {
                if (activeChart != null)
                    DeinitChart();
                activeChart = value;
                NotifyOfPropertyChange();
                InitChart();
            }
        }

        public KeyValuePair<ChartPeriods, System.Action> SelectedPeriod
        {
            get { return selectedPeriod; }
            set
            {
                selectedPeriod = value;
                NotifyOfPropertyChange(nameof(SelectedPeriod));
                DisplayName = $"{Symbol}, {SelectedPeriod.Key}";
                selectedPeriod.Value();
            }
        }

        public IReadOnlyList<IRenderableSeriesViewModel> Series { get; private set; }
        public IReadOnlyList<IndicatorPaneViewModel> Panes { get; private set; }
        public IReadOnlyList<IndicatorViewModel> Indicators { get; private set; }
        public IReadOnlyList<BotControlViewModel> Bots { get; private set; }
        public GenericCommand CloseCommand { get; private set; }

        public bool HasIndicators { get { return Indicators.Count > 0; } }

        public int Precision { get; private set; }
        public string YAxisLabelFormat { get; private set; }

        #endregion

        public string Symbol { get; private set; }
        public UiLock UiLock { get; private set; }

        public override void TryClose(bool? dialogResult = null)
        {
            base.TryClose(dialogResult);

            Indicators.Foreach(i => shell.Agent.AlgoEnv.IdProvider.UnregisterPlugin(i.Model.InstanceId));

            shell.ToolWndManager.CloseWindowByKey(this);

            barChart.Dispose();
            tickChart.Dispose();
        }

        public void OpenOrder()
        {
            shell.OrderCommands.OpenMarkerOrder(Symbol);
        }

        public ChartStorageEntry GetSnapshot()
        {
            return new ChartStorageEntry
            {
                Id = ChartWindowId,
                Symbol = Symbol,
                SelectedPeriod = SelectedPeriod.Key,
                SelectedChartType = Chart.SelectedChartType,
                CrosshairEnabled = Chart.IsCrosshairEnabled,
                Indicators = Indicators.Select(i => new IndicatorStorageEntry
                {
                    Config = i.Model.Setup.Save(),
                }).ToList(),
            };
        }

        public void RestoreFromSnapshot(ChartStorageEntry snapshot)
        {
            if (Symbol != snapshot.Symbol)
            {
                return;
            }

            Chart.SelectedChartType = snapshot.SelectedChartType;
            Chart.IsCrosshairEnabled = snapshot.CrosshairEnabled;
            snapshot.Indicators?.ForEach(i => RestoreIndicator(i));
        }

        private void RestoreIndicator(IndicatorStorageEntry entry)
        {
            if (entry.Config == null)
            {
                logger.Error("Indicator not configured!");
            }
            if (entry.Config.Key == null)
            {
                logger.Error("Indicator key missing!");
            }

            Chart.AddIndicator(entry.Config);
        }

        #region Algo

        public void OpenPlugin(object descriptorObj)
        {
            OpenAlgoSetup((PluginCatalogItem)descriptorObj);
        }

        private void OpenAlgoSetup(PluginCatalogItem item)
        {
            try
            {
                if (item.Descriptor.Type == AlgoTypes.Robot)
                {
                    _botManager.OpenBotSetup(item.Info, Chart);
                    return;
                }

                var model = new SetupPluginViewModel(shell.Agent, item.Key, AlgoTypes.Indicator, Chart.GetSetupContextInfo());
                if (!model.Setup.CanBeSkipped)
                    shell.ToolWndManager.OpenMdiWindow("AlgoSetupWindow", model);
                else
                    AttachPlugin(model);

                model.Closed += AlgoSetupClosed;
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
        }

        private void AttachPlugin(SetupPluginViewModel setupModel)
        {
            if (setupModel == null)
            {
                return;
            }

            switch (setupModel.Setup.Descriptor.Type)
            {
                case AlgoTypes.Indicator:
                    Chart.AddIndicator(setupModel.GetConfig());
                    break;
                default:
                    throw new Exception($"Unknown plugin type '{setupModel.Setup.Descriptor.Type}'");
            }
        }

        void AlgoSetupClosed(SetupPluginViewModel setupModel, bool dlgResult)
        {
            setupModel.Closed -= AlgoSetupClosed;
            if (dlgResult)
                AttachPlugin(setupModel);
        }

        #endregion

        public void Drop(object o)
        {
            var algo = o as AlgoItemViewModel;
            if (algo != null)
            {
                var pluginType = algo.PluginItem.Descriptor.Type;
                if (pluginType == AlgoTypes.Indicator || pluginType == AlgoTypes.Robot)
                    OpenAlgoSetup(algo.PluginItem);
            }
        }

        private void ActivateBarChart(TimeFrames timeFrame, string dateLabelFormat)
        {
            barChart.DateAxisLabelFormat = dateLabelFormat;
            this.Chart = barChart;
            barChart.Activate(timeFrame);
            FilterBots();
        }

        private void ActivateTickChart()
        {
            this.Chart = tickChart;
            tickChart.Activate();
            //barChart.Deactivate();
            FilterBots();
        }

        private void FilterBots()
        {
            Bots = _botsBySymbol.Where(bc => bc.Model.Config.TimeFrame == Chart.TimeFrame).AsObservable();
            NotifyOfPropertyChange(nameof(Bots));
        }

        private void Chart_ParamsLocked()
        {
            shell.ConnectionLock.Lock();
            UiLock.Lock();
        }

        private void Chart_ParamsUnlocked()
        {
            shell.ConnectionLock.Release();
            UiLock.Release();
        }

        private void Indicators_Updated(ListUpdateArgs<IndicatorModel> args)
        {
            NotifyOfPropertyChange("HasIndicators");
            Precision = smb.Descriptor.Precision;
            foreach (var indicator in Indicators.Where(i => i.Model.HasOverlayOutputs))
            {
                Precision = Math.Max(Precision, indicator.Precision);
            }
            UpdateLabelFormat();
        }

        private void InitChart()
        {
            charts.Clear();
            charts.Add(Chart);

            Chart.ParamsLocked += Chart_ParamsLocked;
            Chart.ParamsUnlocked += Chart_ParamsUnlocked;
            Chart.Indicators.Updated += Indicators_Updated;
        }

        private void DeinitChart()
        {
            Chart.ParamsLocked -= Chart_ParamsLocked;
            Chart.ParamsUnlocked -= Chart_ParamsUnlocked;
            Chart.Indicators.Updated -= Indicators_Updated;
        }

        public bool CanDrop(object o)
        {
            return o is AlgoItemViewModel;
        }

        private void UpdateLabelFormat()
        {
            YAxisLabelFormat = $"n{Precision}";
            NotifyOfPropertyChange(nameof(YAxisLabelFormat));
        }
    }
}