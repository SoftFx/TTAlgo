using Machinarium.Qnil;
using Machinarium.Var;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TickTrader.Algo.Backtester;
using TickTrader.Algo.Common.Info;
using TickTrader.Algo.Common.Model;
using TickTrader.Algo.Common.Model.Setup;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Package;

namespace TickTrader.BotTerminal
{
    internal class BacktesterSetupPageViewModel : Page, IPluginIdProvider, IAlgoSetupContext, IAlgoSetupMetadata
    {
        private readonly VarContext _var = new VarContext();
        private readonly TraderClientModel _client;
        private readonly AlgoEnvironment _env;
        private SymbolCatalog _catalog;
        private WindowManager _localWnd;
        private readonly BoolProperty _isDateRangeValid;
        private readonly BoolProperty _allSymbolsValid;
        private readonly BoolVar _isPluginValid;
        private readonly SymbolToken _mainSymbolToken;
        private readonly IReadOnlyList<ISetupSymbolInfo> _observableSymbolTokens;
        private readonly IVarSet<SymbolKey, ISetupSymbolInfo> _symbolTokens;
        private BacktesterPluginSetupViewModel _openedPluginSetup;
        private readonly OptionalItem<TesterModes> _optModeItem;

        public BacktesterSetupPageViewModel(TraderClientModel client, SymbolCatalog catalog, AlgoEnvironment env, BoolVar isRunning)
        {
            DisplayName = "Setup";

            _env = env ?? throw new ArgumentNullException("env");
            _catalog = catalog ?? throw new ArgumentNullException("catalog");
            _client = client;

            _allSymbolsValid = _var.AddBoolProperty();
            //_hasDataToSave = _var.AddBoolProperty();
            //_isRunning = _var.AddBoolProperty();
            //_isVisualizing = _var.AddBoolProperty();

            _localWnd = new WindowManager(this);

            //ActionOverlay = new Property<ActionOverlayViewModel>();
            FeedSymbols = new ObservableCollection<BacktesterSymbolSetupViewModel>();

            DateRange = new DateRangeSelectionViewModel(false);
            IsUpdatingRange = new BoolProperty();
            _isDateRangeValid = new BoolProperty();
            MainTimeFrame = new Property<Feed.Types.Timeframe>();
            MainTimeFrame.Value = Feed.Types.Timeframe.M1;

            SaveResultsToFile = new BoolProperty();
            SaveResultsToFile.Set();

            //_availableSymbols = env.Symbols;

            MainSymbolSetup = CreateSymbolSetupModel(SymbolSetupType.Main);
            MainSymbolShadowSetup = CreateSymbolSetupModel(SymbolSetupType.MainShadow);
            FeedSymbols.Add(MainSymbolShadowSetup);
            UpdateSymbolsState();

            AvailableModels = _var.AddProperty<List<Feed.Types.Timeframe>>();
            SelectedModel = _var.AddProperty<Feed.Types.Timeframe>(Feed.Types.Timeframe.M1);

            ModeProp = _var.AddProperty<OptionalItem<TesterModes>>();
            PluginErrorProp = _var.AddProperty<string>();

            SelectedPlugin = new Property<AlgoPluginViewModel>();
            IsPluginSelected = SelectedPlugin.Var.IsNotNull();
            IsTradeBotSelected = SelectedPlugin.Var.Check(p => p != null && p.Descriptor.IsTradeBot);
            //IsRunning = ActionOverlay.IsRunning;
            //IsStopping = ActionOverlay.IsCancelling;
            _isPluginValid = PluginErrorProp.Var.IsNull();
            IsSetupValid = !IsUpdatingRange.Var & IsPluginSelected & _allSymbolsValid.Var & _isDateRangeValid.Var & _isPluginValid;
            CanSetup = !isRunning & client.IsConnected;
            //CanStop = ActionOverlay.CanCancel;
            //CanSave = !IsRunning & _hasDataToSave.Var;
            //IsVisualizationEnabled = _var.AddBoolProperty();


            Plugins = env.LocalAgentVM.PluginList;

            TradeSettingsSummary = _var.AddProperty<string>();

            _mainSymbolToken = SpecialSymbols.MainSymbolPlaceholder;
            //var predefinedSymbolTokens = new VarList<ISymbolInfo>(new ISymbolInfo[] { _mainSymbolToken });
            var predefinedSymbolTokens = new VarDictionary<SymbolKey, ISetupSymbolInfo>();
            predefinedSymbolTokens.Add(_mainSymbolToken.GetKey(), _mainSymbolToken);

            var existingSymbolTokens = _catalog.AllSymbols.Select((k, s) => (ISetupSymbolInfo)s.ToSymbolToken());
            _symbolTokens = VarCollection.Combine(predefinedSymbolTokens, existingSymbolTokens);

            var sortedSymbolTokens = _symbolTokens.OrderBy((k, v) => k, new SymbolKeyComparer());
            _observableSymbolTokens = sortedSymbolTokens.AsObservable();

            Modes = new List<OptionalItem<TesterModes>>
            {
                new OptionalItem<TesterModes>(TesterModes.Backtesting),
                new OptionalItem<TesterModes>(TesterModes.Visualization),
                new OptionalItem<TesterModes>(TesterModes.Optimization)
            };
            _optModeItem = Modes.Last();
            ModeProp.Value = Modes[0];

            env.LocalAgentVM.Plugins.Updated += a =>
            {
                if (a.Action == DLinqAction.Remove && a.OldItem.Key == SelectedPlugin.Value?.Key)
                    SelectedPlugin.Value = null;
            };

            _var.TriggerOnChange(SelectedPlugin, a =>
            {
                if (a.New != null)
                {
                    var plugin = env.LocalAgent.Library.GetPlugin(a.New.Key);
                    UpdateOptimizationState(plugin.Descriptor_);
                    UpdatePluginState(plugin.Descriptor_);
                    PluginConfig = null;
                    PluginSelected?.Invoke();
                }
                else
                    PluginConfig = null;
            });

            _var.TriggerOnChange(MainSymbolSetup.SelectedTimeframe, a =>
            {
                AvailableModels.Value = EnumHelper.AllValues<Feed.Types.Timeframe>().Where(t => t >= a.New && t != Feed.Types.Timeframe.TicksLevel2).ToList();

                if (_openedPluginSetup != null)
                    _openedPluginSetup.Setup.SelectedTimeFrame = a.New;

                if (SelectedModel.Value < a.New)
                    SelectedModel.Value = a.New;
            });

            _var.TriggerOnChange(MainSymbolSetup.SelectedSymbol, a =>
            {
                if (a.New != null)
                {
                    _mainSymbolToken.Id = a.New.Name;

                    if (_openedPluginSetup != null)
                        _openedPluginSetup.Setup.MainSymbol = a.New.ToSymbolToken();

                    MainSymbolShadowSetup.SelectedSymbolName.Value = a.New.Name;
                }
            });

            client.Connected += () =>
            {
                FeedSymbols.ForEach(s => s.Reset());
            };

            UpdateTradeSummary();
        }

        public BoolVar CanSetup { get; }
        public BoolVar IsSetupValid { get; }
        public BacktesterSettings Settings { get; private set; } = new BacktesterSettings();
        public IObservableList<AlgoPluginViewModel> Plugins { get; private set; }
        public Property<List<Feed.Types.Timeframe>> AvailableModels { get; private set; }
        public Property<Feed.Types.Timeframe> SelectedModel { get; private set; }
        public Property<AlgoPluginViewModel> SelectedPlugin { get; private set; }
        public Property<string> PluginErrorProp { get; }
        public Property<Feed.Types.Timeframe> MainTimeFrame { get; private set; }
        public BacktesterSymbolSetupViewModel MainSymbolSetup { get; private set; }
        public BacktesterSymbolSetupViewModel MainSymbolShadowSetup { get; private set; }
        public PluginConfig PluginConfig { get; private set; }
        //public PluginConfig PluginConfig { get; private set; }
        public Property<string> TradeSettingsSummary { get; private set; }
        //public BoolProperty IsVisualizationEnabled { get; }
        public List<OptionalItem<TesterModes>> Modes { get; }
        public Property<OptionalItem<TesterModes>> ModeProp { get; private set; }
        public TesterModes Mode => ModeProp.Value.Value;
        public BoolProperty SaveResultsToFile { get; }
        public BoolVar IsPluginSelected { get; }
        public BoolVar IsTradeBotSelected { get; }
        public BoolProperty IsUpdatingRange { get; private set; }
        public DateRangeSelectionViewModel DateRange { get; }
        public BoolVar IsDateRangeEnabled => _isDateRangeValid.Var;
        public ObservableCollection<BacktesterSymbolSetupViewModel> FeedSymbols { get; private set; }
        public event System.Action PluginSelected;

        public async void OpenTradeSetup()
        {
            var setup = new BacktesterTradeSetupViewModel(Settings, _client.SortedCurrenciesNames);

            _localWnd.OpenMdiWindow(SetupWndKey, setup);

            if (await setup.Result)
            {
                Settings = setup.GetSettings();
                UpdateTradeSummary();
            }
        }

        public async Task PrecacheData(IActionObserver observer, CancellationToken cToken, DateTime from, DateTime to)
        {
            await MainSymbolSetup.PrecacheData(observer, cToken, from, to, SelectedModel.Value);

            foreach (var symbolSetup in FeedSymbols)
                await symbolSetup.PrecacheData(observer, cToken, from, to);
        }

        public void InitToken()
        {
            var mainSymbol = MainSymbolSetup.SelectedSymbol.Value;
            _mainSymbolToken.Id = mainSymbol.Name;
        }

        public void Apply(Backtester backtester, DateTime from, DateTime to, bool isVisualizing)
        {
            MainSymbolSetup.Apply(backtester, from, to, SelectedModel.Value, isVisualizing);

            foreach (var symbolSetup in FeedSymbols)
                symbolSetup.Apply(backtester, from, to, isVisualizing);

            foreach (var rec in _client.Currencies.Snapshot)
                backtester.CommonSettings.Currencies.Add(rec.Key, rec.Value);

            Settings.Apply(backtester);
        }

        public void Apply(Optimizer optimizer, DateTime from, DateTime to)
        {
            MainSymbolSetup.Apply(optimizer, from, to, SelectedModel.Value);

            foreach (var symbolSetup in FeedSymbols)
                symbolSetup.Apply(optimizer, from, to);

            foreach (var rec in _client.Currencies.Snapshot)
                optimizer.CommonSettings.Currencies.Add(rec.Key, rec.Value);

            Settings.Apply(optimizer.CommonSettings);
        }

        #region Saving Results

        public void SaveTestSetupAsText(PluginDescriptor pDescriptor, PluginConfig config, System.IO.Stream stream, DateTime from, DateTime to)
        {
            using (var writer = new System.IO.StreamWriter(stream))
            {
                writer.WriteLine(FeedSetupToText(from, to));
                writer.WriteLine(TradeSetupToText());
                writer.WriteLine(PluginSetupToText(pDescriptor, config, false));
            }
        }

        private string FeedSetupToText(DateTime from, DateTime to)
        {
            var writer = new StringBuilder();

            writer.AppendLine("Main symbol: " + MainSymbolSetup.AsText());
            writer.AppendLine("Model: based on " + SelectedModel.Value);

            writer.AppendLine("Symbols data feed: " + MainSymbolSetup.AsText());
            foreach (var addSymbols in FeedSymbols)
                writer.AppendLine("+Symbol " + addSymbols.AsText());

            writer.AppendFormat("Period: {0} to {1}", from.ToShortDateString(), to.ToShortDateString());

            return writer.ToString();
        }

        private string TradeSetupToText()
        {
            return Settings.ToText(false);
        }

        private string PluginSetupToText(PluginDescriptor pDescriptor, PluginConfig config, bool compact)
        {
            var writer = new StringBuilder();

            if (pDescriptor.IsIndicator)
                writer.AppendFormat("Indicator: {0} v{1}", pDescriptor.DisplayName, pDescriptor.Version).AppendLine();
            else if (pDescriptor.IsTradeBot)
                writer.AppendFormat("Trade Bot: {0} v{1}", pDescriptor.DisplayName, pDescriptor.Version).AppendLine();

            //int count = 0;
            //foreach (var param in setup.Parameters)
            //{
            //    if (compact && count > 0)
            //        writer.Append(", ");
            //    writer.AppendFormat("{0}={1}", param.DisplayName, param.GetQuotedValue());
            //    if (!compact)
            //        writer.AppendLine();
            //    count++;
            //}

            //foreach (var input in setup.Inputs)
            //{
            //    if (compact)
            //        writer.Append(' ');
            //    writer.AppendFormat("{0} = {1}", input.DisplayName, input.ValueAsText);
            //    if (!compact)
            //        writer.AppendLine();
            //}

            return writer.ToString();
        }

        #endregion

        [Conditional("DEBUG")]
        public void PrintCacheData()
        {
            MainSymbolSetup.PrintCacheData(SelectedModel.Value);
        }

        #region Plugin Setup

        private const string SetupWndKey = "SetupAuxWnd";

        public void OpenPluginSetup()
        {
            _localWnd.OpenOrActivateWindow(SetupWndKey, () =>
            {
                _openedPluginSetup = PluginConfig == null
                    ? new BacktesterPluginSetupViewModel(_env.LocalAgent, SelectedPlugin.Value.PluginInfo, this, this.GetSetupContextInfo())
                    : new BacktesterPluginSetupViewModel(_env.LocalAgent, SelectedPlugin.Value.PluginInfo, this, this.GetSetupContextInfo(), PluginConfig);
                //_localWnd.OpenMdiWindow(wndKey, _openedPluginSetup);
                _openedPluginSetup.Setup.MainSymbol = MainSymbolSetup.SelectedSymbol.Value.ToSymbolToken();
                _openedPluginSetup.Setup.SelectedTimeFrame = MainSymbolSetup.SelectedTimeframe.Value;
                _openedPluginSetup.Closed += PluginSetupClosed;
                _openedPluginSetup.Setup.ConfigLoaded += Setup_ConfigLoaded;
                return _openedPluginSetup;
            });
        }

        public void CloseSetupDialog()
        {
            _localWnd.CloseWindowByKey(SetupWndKey);
        }

        private void PluginSetupClosed(BacktesterPluginSetupViewModel setup, bool dlgResult)
        {
            if (dlgResult)
                PluginConfig = setup.GetConfig();

            setup.Closed -= PluginSetupClosed;
            setup.Setup.ConfigLoaded -= Setup_ConfigLoaded;

            _openedPluginSetup = null;
        }

        private void Setup_ConfigLoaded(PluginConfigViewModel config)
        {
            MainSymbolSetup.SelectedSymbol.Value = _catalog.GetSymbol(config.MainSymbol);
            MainSymbolSetup.SelectedSymbolName.Value = MainSymbolSetup.SelectedSymbol.Value.Name;
            MainSymbolSetup.SelectedTimeframe.Value = config.SelectedTimeFrame;
        }

        #endregion

        private void AddSymbol()
        {
            FeedSymbols.Add(CreateSymbolSetupModel(SymbolSetupType.Additional));
            UpdateSymbolsState();
        }

        private BacktesterSymbolSetupViewModel CreateSymbolSetupModel(SymbolSetupType type, Var<SymbolData> symbolSrc = null)
        {
            var smb = new BacktesterSymbolSetupViewModel(type, _catalog.ObservableSymbols, symbolSrc);
            smb.Removed += Smb_Removed;
            smb.OnAdd += AddSymbol;

            smb.IsUpdating.PropertyChanged += IsUpdating_PropertyChanged;
            smb.IsSymbolSelected.PropertyChanged += IsSymbolSelected_PropertyChanged;

            return smb;
        }

        private void Smb_Removed(BacktesterSymbolSetupViewModel smb)
        {
            FeedSymbols.Remove(smb);
            smb.IsUpdating.PropertyChanged -= IsUpdating_PropertyChanged;
            smb.IsSymbolSelected.PropertyChanged -= IsSymbolSelected_PropertyChanged;
            smb.Removed -= Smb_Removed;

            UpdateRangeState();
            UpdateSymbolsState();
        }

        private void UpdateRangeState()
        {
            var allSymbols = FeedSymbols;

            IsUpdatingRange.Value = allSymbols.Any(s => s.IsUpdating.Value);

            var max = allSymbols.Max(s => s.AvailableRange.Value?.Item2);
            var min = allSymbols.Min(s => s.AvailableRange.Value?.Item1);

            if (max != null && min != null)
            {
                bool wasEmpty = DateRange.From == DateTime.MinValue;

                DateRange.UpdateBoundaries(min ?? DateTime.MinValue, max ?? DateTime.MaxValue);
                _isDateRangeValid.Set();

                if (wasEmpty)
                    DateRange.ResetSelectedRange();
            }
            else
                _isDateRangeValid.Clear();
        }

        private void UpdateSymbolsState()
        {
            _allSymbolsValid.Value = GetAllSymbols().All(s => s.IsValid.Value);
        }

        public IEnumerable<BacktesterSymbolSetupViewModel> GetAllSymbols()
        {
            yield return MainSymbolSetup;

            foreach (var smb in FeedSymbols)
                yield return smb;
        }

        private void IsUpdating_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            UpdateRangeState();
            UpdateSymbolsState();
        }

        private void IsSymbolSelected_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            UpdateSymbolsState();
        }

        public void CheckDuplicateSymbols()
        {
            var unique = new HashSet<string>();

            foreach (var smb in FeedSymbols)
            {
                var name = smb.SelectedSymbolName.Value;

                if (unique.Contains(name))
                    throw new Exception("Duplicate symbol: " + name);

                unique.Add(name);
            }
        }

        private void UpdateTradeSummary()
        {
            if (Settings.AccType == AccountInfo.Types.Type.Gross || Settings.AccType == AccountInfo.Types.Type.Net)
                TradeSettingsSummary.Value = string.Format("{0} {1} {2} L={3}, D={4}, {5}ms", Settings.AccType,
                    Settings.InitialBalance, Settings.BalanceCurrency, Settings.Leverage, "Default", Settings.ServerPingMs);
        }

        public override void TryClose(bool? dialogResult = null)
        {
            base.TryClose(dialogResult);

            _var.Dispose();
        }

        private void UpdateOptimizationState(PluginDescriptor descriptor)
        {
            _optModeItem.IsEnabled = descriptor.IsTradeBot;
            if (ModeProp.Value == _optModeItem)
                ModeProp.Value = Modes[0];
        }

        private void UpdatePluginState(PluginDescriptor descriptor)
        {
            if (descriptor.IsValid)
                PluginErrorProp.Value = null;
            else
                PluginErrorProp.Value = descriptor.Error.ToString();
        }


        #region IAlgoSetupMetadata

        IReadOnlyList<ISetupSymbolInfo> IAlgoSetupMetadata.Symbols => _observableSymbolTokens;

        MappingCollectionInfo IAlgoSetupMetadata.Mappings => _env.LocalAgent.Mappings;

        IPluginIdProvider IAlgoSetupMetadata.IdProvider => this;

        #endregion

        #region IPluginIdProvider

        string IPluginIdProvider.GeneratePluginId(PluginDescriptor descriptor)
        {
            return descriptor.DisplayName;
        }

        bool IPluginIdProvider.IsValidPluginId(Metadata.Types.PluginType pluginType, string pluginId)
        {
            return true;
        }

        void IPluginIdProvider.RegisterPluginId(string pluginId)
        {
            return;
        }

        #endregion IPluginIdProvider

        #region IAlgoSetupContext

        Feed.Types.Timeframe IAlgoSetupContext.DefaultTimeFrame => MainTimeFrame.Value;

        ISetupSymbolInfo IAlgoSetupContext.DefaultSymbol => _mainSymbolToken;

        MappingKey IAlgoSetupContext.DefaultMapping => MappingDefaults.DefaultBarToBarMapping.Key;

        #endregion IAlgoSetupContext
    }

    public class OptionalItem<T> : ObservableObject
    {
        private bool _enabled;

        public OptionalItem(T value, bool enabled = true)
        {
            Value = value;
            _enabled = enabled;
        }

        public T Value { get; }

        public bool IsEnabled
        {
            get => _enabled;
            set
            {
                if (_enabled != value)
                {
                    _enabled = value;
                    NotifyOfPropertyChange(nameof(IsEnabled));
                }
            }
        }
    }

    public enum TesterModes
    {
        Backtesting,
        Visualization,
        Optimization
    }
}
