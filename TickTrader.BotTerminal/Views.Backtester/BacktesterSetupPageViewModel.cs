using Caliburn.Micro;
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
using TickTrader.Algo.Api;
using TickTrader.Algo.Common.Info;
using TickTrader.Algo.Common.Model;
using TickTrader.Algo.Common.Model.Setup;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Metadata;
using TickTrader.Algo.Core.Repository;

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
        private readonly IReadOnlyList<ISymbolInfo> _observableSymbolTokens;
        private readonly IVarSet<SymbolKey, ISymbolInfo> _symbolTokens;
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
            AdditionalSymbols = new ObservableCollection<BacktesterSymbolSetupViewModel>();

            DateRange = new DateRangeSelectionViewModel(false);
            IsUpdatingRange = new BoolProperty();
            _isDateRangeValid = new BoolProperty();
            MainTimeFrame = new Property<TimeFrames>();
            MainTimeFrame.Value = TimeFrames.M1;

            SaveResultsToFile = new BoolProperty();
            SaveResultsToFile.Set();

            //_availableSymbols = env.Symbols;

            MainSymbolSetup = CreateSymbolSetupModel(SymbolSetupType.Main);
            UpdateSymbolsState();

            AvailableModels = _var.AddProperty<List<TimeFrames>>();
            SelectedModel = _var.AddProperty<TimeFrames>(TimeFrames.M1);

            ModeProp = _var.AddProperty<OptionalItem<TesterModes>>();
            PluginErrorProp = _var.AddProperty<string>();

            SelectedPlugin = new Property<AlgoPluginViewModel>();
            IsPluginSelected = SelectedPlugin.Var.IsNotNull();
            IsTradeBotSelected = SelectedPlugin.Var.Check(p => p != null && p.Descriptor.Type == AlgoTypes.Robot);
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
            var predefinedSymbolTokens = new VarDictionary<SymbolKey, ISymbolInfo>();
            predefinedSymbolTokens.Add(_mainSymbolToken.GetKey(), _mainSymbolToken);

            var existingSymbolTokens = _catalog.AllSymbols.Select((k, s) => (ISymbolInfo)s.ToSymbolToken());
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
                    var pluginRef = env.LocalAgent.Library.GetPluginRef(a.New.Key);
                    UpdateOptimizationState(pluginRef.Metadata.Descriptor);
                    UpdatePluginState(pluginRef.Metadata.Descriptor);
                    PluginSetup = new PluginSetupModel(pluginRef, this, this);
                    PluginSelected?.Invoke();
                }
                else
                    PluginSetup = null;
            });

            _var.TriggerOnChange(MainSymbolSetup.SelectedTimeframe, a =>
            {
                AvailableModels.Value = EnumHelper.AllValues<TimeFrames>().Where(t => t >= a.New).ToList();

                if (_openedPluginSetup != null)
                    _openedPluginSetup.Setup.SelectedTimeFrame = a.New;

                if (SelectedModel.Value < a.New)
                    SelectedModel.Value = a.New;
            });

            _var.TriggerOnChange(SelectedModel, a =>
            {
                MainSymbolSetup.UpdateAvailableRange(SelectedModel.Value);
            });

            _var.TriggerOnChange(MainSymbolSetup.SelectedSymbol, a =>
            {
                if (a.New != null)
                {
                    _mainSymbolToken.Id = a.New.Name;

                    if (_openedPluginSetup != null)
                        _openedPluginSetup.Setup.MainSymbol = a.New.ToSymbolToken();

                    MainSymbolSetup.UpdateAvailableRange(SelectedModel.Value);
                }
            });

            client.Connected += () =>
            {
                GetAllSymbols().Foreach(s => s.Reset());
                MainSymbolSetup.UpdateAvailableRange(SelectedModel.Value);
            };

            UpdateTradeSummary();
        }

        public BoolVar CanSetup { get; }
        public BoolVar IsSetupValid { get; }
        public BacktesterSettings Settings { get; private set; } = new BacktesterSettings();
        public IObservableList<AlgoPluginViewModel> Plugins { get; private set; }
        public Property<List<TimeFrames>> AvailableModels { get; private set; }
        public Property<TimeFrames> SelectedModel { get; private set; }
        public Property<AlgoPluginViewModel> SelectedPlugin { get; private set; }
        public Property<string> PluginErrorProp { get; }
        public Property<TimeFrames> MainTimeFrame { get; private set; }
        public BacktesterSymbolSetupViewModel MainSymbolSetup { get; private set; }
        public PluginSetupModel PluginSetup { get; private set; }
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
        public ObservableCollection<BacktesterSymbolSetupViewModel> AdditionalSymbols { get; private set; }
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

            foreach (var symbolSetup in AdditionalSymbols)
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

            foreach (var symbolSetup in AdditionalSymbols)
                symbolSetup.Apply(backtester, from, to, isVisualizing);

            foreach (var rec in _client.Currencies.Snapshot)
                backtester.CommonSettings.Currencies.Add(rec.Key, rec.Value);

            Settings.Apply(backtester);
        }

        public void Apply(Optimizer optimizer, DateTime from, DateTime to)
        {
            MainSymbolSetup.Apply(optimizer, from, to, SelectedModel.Value);

            foreach (var symbolSetup in AdditionalSymbols)
                symbolSetup.Apply(optimizer, from, to);

            foreach (var rec in _client.Currencies.Snapshot)
                optimizer.CommonSettings.Currencies.Add(rec.Key, rec.Value);

            Settings.Apply(optimizer.CommonSettings);
        }

        #region Saving Results

        public void SaveTestSetupAsText(PluginSetupModel setup, System.IO.Stream stream, DateTime from, DateTime to)
        {
            var dPlugin = setup.Metadata.Descriptor;

            using (var writer = new System.IO.StreamWriter(stream))
            {
                writer.WriteLine(FeedSetupToText(setup, from, to));
                writer.WriteLine(TradeSetupToText());
                writer.WriteLine(PluginSetupToText(setup, false));
            }
        }

        private string FeedSetupToText(PluginSetupModel setup, DateTime from, DateTime to)
        {
            var writer = new StringBuilder();

            writer.AppendLine("Main symbol: " + MainSymbolSetup.AsText());
            writer.AppendLine("Model: based on " + SelectedModel.Value);

            foreach (var addSymbols in AdditionalSymbols)
                writer.AppendLine("+Symbol " + addSymbols.AsText());

            writer.AppendFormat("Period: {0} to {1}", from.ToShortDateString(), to.ToShortDateString());

            return writer.ToString();
        }

        private string TradeSetupToText()
        {
            return Settings.ToText(false);
        }

        private string PluginSetupToText(PluginSetupModel setup, bool compact)
        {
            var writer = new StringBuilder();
            var dPlugin = setup.Metadata.Descriptor;

            if (dPlugin.Type == AlgoTypes.Indicator)
                writer.AppendFormat("Indicator: {0} v{1}", dPlugin.DisplayName, dPlugin.Version).AppendLine();
            else if (dPlugin.Type == AlgoTypes.Robot)
                writer.AppendFormat("Trade Bot: {0} v{1}", dPlugin.DisplayName, dPlugin.Version).AppendLine();

            int count = 0;
            foreach (var param in setup.Parameters)
            {
                if (compact && count > 0)
                    writer.Append(", ");
                writer.AppendFormat("{0}={1}", param.DisplayName, param.GetQuotedValue());
                if (!compact)
                    writer.AppendLine();
                count++;
            }

            foreach (var input in setup.Inputs)
            {
                if (compact)
                    writer.Append(' ');
                writer.AppendFormat("{0} = {1}", input.DisplayName, input.ValueAsText);
                if (!compact)
                    writer.AppendLine();
            }

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
                _openedPluginSetup = PluginSetup == null
                    ? new BacktesterPluginSetupViewModel(_env.LocalAgent, SelectedPlugin.Value.Info, this, this.GetSetupContextInfo())
                    : new BacktesterPluginSetupViewModel(_env.LocalAgent, SelectedPlugin.Value.Info, this, this.GetSetupContextInfo(), PluginSetup.Save());
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
                PluginSetup.Load(setup.GetConfig());

            setup.Closed -= PluginSetupClosed;
            setup.Setup.ConfigLoaded -= Setup_ConfigLoaded;

            _openedPluginSetup = null;
        }

        private void Setup_ConfigLoaded(PluginConfigViewModel config)
        {
            MainSymbolSetup.SelectedSymbol.Value = _catalog.GetSymbol(config.MainSymbol);
            MainSymbolSetup.SelectedTimeframe.Value = config.SelectedTimeFrame;
        }

        #endregion

        private void AddSymbol()
        {
            AdditionalSymbols.Add(CreateSymbolSetupModel(SymbolSetupType.Additional));
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
            AdditionalSymbols.Remove(smb);
            smb.IsUpdating.PropertyChanged -= IsUpdating_PropertyChanged;
            smb.IsSymbolSelected.PropertyChanged -= IsSymbolSelected_PropertyChanged;
            smb.Removed -= Smb_Removed;

            UpdateRangeState();
            UpdateSymbolsState();
        }

        private void UpdateRangeState()
        {
            var allSymbols = GetAllSymbols();

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

            foreach (var smb in AdditionalSymbols)
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
            unique.Add(MainSymbolSetup.SelectedSymbol.Value.Name);

            foreach (var smb in AdditionalSymbols)
            {
                var name = smb.SelectedSymbol.Value.Name;

                if (unique.Contains(name))
                    throw new Exception("Duplicate symbol: " + name);

                unique.Add(name);
            }
        }

        private void UpdateTradeSummary()
        {
            if (Settings.AccType == AccountTypes.Gross || Settings.AccType == AccountTypes.Net)
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
            _optModeItem.IsEnabled = descriptor.Type == AlgoTypes.Robot;
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

        IReadOnlyList<ISymbolInfo> IAlgoSetupMetadata.Symbols => _observableSymbolTokens;

        MappingCollection IAlgoSetupMetadata.Mappings => _env.LocalAgent.Mappings;

        IPluginIdProvider IAlgoSetupMetadata.IdProvider => this;

        #endregion

        #region IPluginIdProvider

        string IPluginIdProvider.GeneratePluginId(PluginDescriptor descriptor)
        {
            return descriptor.DisplayName;
        }

        bool IPluginIdProvider.IsValidPluginId(AlgoTypes pluginType, string pluginId)
        {
            return true;
        }

        void IPluginIdProvider.RegisterPluginId(string pluginId)
        {
            return;
        }

        #endregion IPluginIdProvider

        #region IAlgoSetupContext

        TimeFrames IAlgoSetupContext.DefaultTimeFrame => MainTimeFrame.Value;

        ISymbolInfo IAlgoSetupContext.DefaultSymbol => _mainSymbolToken;

        MappingKey IAlgoSetupContext.DefaultMapping => new MappingKey(MappingCollection.DefaultFullBarToBarReduction);

        #endregion IAlgoSetupContext
    }

    public enum TesterModes
    {
        Backtesting,
        Visualization,
        Optimization
    }
}
