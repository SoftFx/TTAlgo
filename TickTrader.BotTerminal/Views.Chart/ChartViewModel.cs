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

namespace TickTrader.BotTerminal
{
    class ChartViewModel : Screen, IDropHandler
    {
        private static int idSeed;
        private readonly Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private readonly TraderClientModel clientModel;
        private readonly AlgoEnvironment algoEnv;
        private ChartModelBase activeChart;
        private readonly BarChartModel barChart;
        private readonly TickChartModel tickChart;
        private readonly IShell shell;
        private readonly VarList<BotControlViewModel> bots = new VarList<BotControlViewModel>();
        private readonly VarList<ChartModelBase> charts = new VarList<ChartModelBase>();
        private readonly SymbolModel smb;
        private PreferencesStorageModel _preferences;

        public ChartViewModel(string symbol, string period, IShell shell, TraderClientModel clientModel, AlgoEnvironment algoEnv, PersistModel storage)
        {
            this.Symbol = symbol;
            this.DisplayName = symbol;
            this.clientModel = clientModel;
            this.algoEnv = algoEnv;
            this.shell = shell;
            _preferences = storage.PreferencesStorage.StorageModel;

            ChartWindowId = "Chart" + ++idSeed;

            smb = clientModel.Symbols.GetOrDefault(symbol);

            Precision = smb.Descriptor.Precision;
            UpdateLabelFormat();

            this.barChart = new BarChartModel(smb, algoEnv, clientModel);
            this.tickChart = new TickChartModel(smb, algoEnv, clientModel);
            this.UiLock = new UiLock();

            var allIndicators = charts.SelectMany(c => c.Indicators);
            var dataSeries = charts.SelectMany(c => c.DataSeriesCollection);
            var indicatorViewModels = allIndicators.Chain().Select(i => new IndicatorViewModel(Chart, i, ChartWindowId, smb));
            var overlayIndicators = indicatorViewModels.Chain().Where(i => i.Model.HasOverlayOutputs);
            var overlaySeries = overlayIndicators.Chain().SelectMany(i => i.Series);
            var allSeries = VarCollection.CombineChained(dataSeries, overlaySeries);
            var paneIndicators = indicatorViewModels.Chain().Where(i => i.Model.HasPaneOutputs);
            var panes = paneIndicators.Chain().SelectMany(i => i.Panes);

            Series = allSeries.AsObservable();

            Indicators = indicatorViewModels.AsObservable();
            Panes = panes.AsObservable();
            Bots = bots.AsObservable();

            periodActivatos.Add("MN1", () => ActivateBarChart(TimeFrames.MN, "MMMM yyyy"));
            periodActivatos.Add("W1", () => ActivateBarChart(TimeFrames.W, "d MMMM yyyy"));
            periodActivatos.Add("D1", () => ActivateBarChart(TimeFrames.D, "d MMMM yyyy"));
            periodActivatos.Add("H4", () => ActivateBarChart(TimeFrames.H4, "d MMMM yyyy HH:mm"));
            periodActivatos.Add("H1", () => ActivateBarChart(TimeFrames.H1, "d MMMM yyyy HH:mm"));
            periodActivatos.Add("M30", () => ActivateBarChart(TimeFrames.M30, "d MMMM yyyy HH:mm"));
            periodActivatos.Add("M15", () => ActivateBarChart(TimeFrames.M15, "d MMMM yyyy HH:mm"));
            periodActivatos.Add("M5", () => ActivateBarChart(TimeFrames.M5, "d MMMM yyyy HH:mm"));
            periodActivatos.Add("M1", () => ActivateBarChart(TimeFrames.M1, "d MMMM yyyy HH:mm"));
            periodActivatos.Add("S10", () => ActivateBarChart(TimeFrames.S10, "d MMMM yyyy HH:mm:ss"));
            periodActivatos.Add("S1", () => ActivateBarChart(TimeFrames.S1, "d MMMM yyyy HH:mm:ss"));
            periodActivatos.Add("Ticks", () => ActivateTickChart());

            SelectedPeriod = periodActivatos.ContainsKey(period) ? periodActivatos.FirstOrDefault(p => p.Key == period) : periodActivatos.ElementAt(8);

            CloseCommand = new GenericCommand(o => TryClose());
        }

        #region Bindable Properties

        private readonly Dictionary<string, System.Action> periodActivatos = new Dictionary<string, System.Action>();
        private KeyValuePair<string, System.Action> selectedPeriod;

        public string ChartWindowId { get; private set; }

        public Dictionary<string, System.Action> AvailablePeriods { get { return periodActivatos; } }

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

        public KeyValuePair<string, System.Action> SelectedPeriod
        {
            get { return selectedPeriod; }
            set
            {
                selectedPeriod = value;
                NotifyOfPropertyChange("SelectedPeriod");
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
        public bool HasStartedBots => Bots.Any(b => b.IsStarted);

        public int Precision { get; private set; }
        public string YAxisLabelFormat { get; private set; }

        #endregion

        public string Symbol { get; private set; }
        public UiLock UiLock { get; private set; }

        public override void TryClose(bool? dialogResult = null)
        {
            if (dialogResult == null && HasStartedBots)
            {
                var closeDlg = new CloseChartDialogViewModel(this);
                shell.ToolWndManager.ShowDialog(closeDlg, this);
                dialogResult = closeDlg.IsConfirmed;
            }

            if (dialogResult == false)
            {
                return;
            }

            StopBots();

            base.TryClose(dialogResult);

            Indicators.Foreach(i => algoEnv.IdProvider.RemovePlugin(i.Model.InstanceId));
            Bots.Foreach(b => algoEnv.IdProvider.RemovePlugin(b.Model.InstanceId));

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
                Symbol = Symbol,
                SelectedPeriod = SelectedPeriod.Key,
                SelectedChartType = Chart.SelectedChartType,
                CrosshairEnabled = Chart.IsCrosshairEnabled,
                Indicators = Indicators.Select(i => new IndicatorStorageEntry
                {
                    DescriptorId = i.Model.Setup.Descriptor.Id,
                    PluginFilePath = i.Model.PluginFilePath,
                    InstanceId = i.Model.InstanceId,
                    Isolated = i.Model.Isolated,
                    Config = i.Model.Setup.Save(),
                    Permissions = i.Model.Permissions,
                }).ToList(),
                Bots = Bots.Select(b => new TradeBotStorageEntry
                {
                    DescriptorId = b.Model.Setup.Descriptor.Id,
                    PluginFilePath = b.Model.PluginFilePath,
                    InstanceId = b.Model.InstanceId,
                    Isolated = b.Model.Isolated,
                    Started = b.Model.State == BotModelStates.Running,
                    Config = b.Model.Setup.Save(),
                    Permissions = b.Model.Permissions,
                    StateViewOpened = b.Model.StateViewOpened,
                    StateSettings = b.Model.StateViewSettings.StorageModel,
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
            snapshot.Bots?.ForEach(b => RestoreTradeBot(b));
        }

        private PluginSetupViewModel RestorePlugin<T>(PluginStorageEntry<T> snapshot) where T : PluginStorageEntry<T>, new()
        {
            var catalogItem = algoEnv.Repo.AllPlugins.Where((k, i) => i.CanBeUseForSnapshot(snapshot)).Snapshot.FirstOrDefault().Value;
            if (catalogItem == null)
            {
                return null;
            }
            var setupModel = new PluginSetupViewModel(algoEnv, catalogItem, Chart)
            {
                InstanceId = snapshot.InstanceId,
                Isolated = snapshot.Isolated,
                Permissions = snapshot.Permissions,
            };
            if (snapshot is TradeBotStorageEntry)
            {
                setupModel.RunBot = (snapshot as TradeBotStorageEntry).Started && _preferences.RestartBotsOnStartup;
            }
            if (snapshot.Config != null)
            {
                setupModel.Setup.Load(snapshot.Config);
            }
            return setupModel;
        }

        private void RestoreIndicator(IndicatorStorageEntry entry)
        {
            var setupModel = RestorePlugin(entry);

            if (setupModel == null)
            {
                logger.Error($"Indicator '{entry.DescriptorId}' from {entry.PluginFilePath} not found!");
                return;
            }

            if (setupModel.Setup.Descriptor.AlgoLogicType != AlgoTypes.Indicator)
            {
                logger.Error($"Plugin '{entry.DescriptorId}' from {entry.PluginFilePath} is not an indicator!");
            }

            AttachPlugin(setupModel);
        }

        private void RestoreTradeBot(TradeBotStorageEntry entry)
        {
            var setupModel = RestorePlugin(entry);

            if (setupModel == null)
            {
                logger.Error($"Trade bot '{entry.DescriptorId}' from {entry.PluginFilePath} not found!");
                return;
            }

            if (setupModel.Setup.Descriptor.AlgoLogicType != AlgoTypes.Robot)
            {
                logger.Error($"Plugin '{entry.DescriptorId}' from {entry.PluginFilePath} is not a trade bot!");
                return;
            }

            //AttachTradeBot(setupModel, entry.StateSettings.Clone(), entry.StateViewOpened);
            AttachTradeBot(setupModel, new WindowStorageModel { Width = 300, Height = 300 }, false);
        }

        #region Algo

        public void OpenPlugin(object descriptorObj)
        {
            OpenAlgoSetup((PluginCatalogItem)descriptorObj);
        }

        public void StopBots()
        {
            foreach (var bot in Bots)
            {
                if (bot.IsStarted)
                {
                    bot.StartStop();
                }
                shell.ToolWndManager.CloseWindowByKey(bot.Model);
            }
        }

        private void OpenAlgoSetup(PluginCatalogItem item)
        {
            try
            {
                var model = new PluginSetupViewModel(algoEnv, item, Chart);
                if (!model.SetupCanBeSkipped)
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

        private void AttachPlugin(PluginSetupViewModel setupModel)
        {
            if (setupModel == null)
            {
                return;
            }

            var pluginType = setupModel.Setup.Descriptor.AlgoLogicType;

            if (pluginType == AlgoTypes.Indicator)
            {
                Chart.AddIndicator(setupModel);
            }
            else if (pluginType == AlgoTypes.Robot)
            {
                AttachTradeBot(setupModel, new WindowStorageModel { Width = 300, Height = 300 }, true);
            }
        }

        private void AttachTradeBot(PluginSetupViewModel setupModel, WindowStorageModel stateSettings, bool openState)
        {
            var bot = new TradeBotModel(setupModel, Chart, stateSettings);
            var viewModel = new BotControlViewModel(bot, shell.ToolWndManager, setupModel.RunBot, openState);
            viewModel.Closed += BotClosed;
            bots.Add(viewModel);
            algoEnv.IdProvider.AddPlugin(bot);
        }

        void AlgoSetupClosed(PluginSetupViewModel setupModel, bool dlgResult)
        {
            setupModel.Closed -= AlgoSetupClosed;
            if (dlgResult)
                AttachPlugin(setupModel);
        }

        private void BotClosed(BotControlViewModel sender)
        {
            bots.Remove(sender);
            algoEnv.IdProvider.RemovePlugin(sender.Model.InstanceId);
            sender.Dispose();
            sender.Closed -= BotClosed;
            //sender.Model.StateChanged -= Bot_StateChanged;
        }

        #endregion

        public void Drop(object o)
        {
            var algo = o as AlgoItemViewModel;
            if (algo != null)
            {
                var pluginType = algo.PluginItem.Descriptor.AlgoLogicType;
                if (pluginType == AlgoTypes.Indicator || pluginType == AlgoTypes.Robot)
                    OpenAlgoSetup(algo.PluginItem);
            }
        }

        private void ActivateBarChart(TimeFrames timeFrame, string dateLabelFormat)
        {
            barChart.DateAxisLabelFormat = dateLabelFormat;
            this.Chart = barChart;
            barChart.Activate(timeFrame);
        }

        private void ActivateTickChart()
        {
            this.Chart = tickChart;
            tickChart.Activate();
            //barChart.Deactivate();
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