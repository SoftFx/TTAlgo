using Caliburn.Micro;
using Machinarium.Qnil;
using Machinarium.Var;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TickTrader.Algo.BacktesterApi;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Core.Setup;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Package;
using TickTrader.Algo.Server.PublicAPI.Converters;
using TickTrader.FeedStorage;
using TickTrader.FeedStorage.Api;

namespace TickTrader.BotTerminal
{
    internal record PluginNameAndGroup(string DisplayName, AlgoPluginViewModel.GroupType Group);


    internal sealed class BacktesterSetupPageViewModel : Page, IPluginIdProvider, IAlgoSetupContext, IAlgoSetupMetadata
    {
        private readonly ReadOnlyCollection<ISetupSymbolInfo> _observableSymbolTokens;
        private readonly OptionalItem<BacktesterMode> _optModeItem;
        private readonly TraderClientModel _client;

        private readonly VarContext _var = new();

        private readonly SymbolToken _mainSymbolToken;
        private readonly ISymbolCatalog _catalog;
        private readonly WindowManager _localWnd;
        private readonly AlgoEnvironment _env;

        private readonly BoolProperty _isDateRangeValid;
        private readonly BoolProperty _allSymbolsValid;
        private readonly BoolVar _isPluginValid;


        private BacktesterPluginSetupViewModel _openedPluginSetup;


        public BacktesterSetupPageViewModel(TraderClientModel client, ISymbolCatalog catalog, AlgoEnvironment env, BoolVar isRunning)
        {
            DisplayName = "Setup";

            _env = env ?? throw new ArgumentNullException(nameof(env));
            _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
            _client = client;

            _allSymbolsValid = _var.AddBoolProperty();
            //_hasDataToSave = _var.AddBoolProperty();
            //_isRunning = _var.AddBoolProperty();

            _localWnd = new WindowManager(this);

            //ActionOverlay = new Property<ActionOverlayViewModel>();
            FeedSymbols = new ObservableCollection<BacktesterSymbolSetupViewModel>();

            DateRange = new DateRangeSelectionViewModel(false);
            IsUpdatingRange = new BoolProperty();
            _isDateRangeValid = new BoolProperty();

            //_availableSymbols = env.Symbols;

            MainSymbolSetup = CreateSymbolSetupModel(SymbolSetupType.Main);
            MainSymbolShadowSetup = CreateSymbolSetupModel(SymbolSetupType.MainShadow);
            FeedSymbols.Add(MainSymbolShadowSetup);
            UpdateSymbolsState();

            AvailableModels = _var.AddProperty<List<Feed.Types.Timeframe>>();
            SelectedModel = _var.AddProperty(Feed.Types.Timeframe.M1);

            ModeProp = _var.AddProperty<OptionalItem<BacktesterMode>>();
            PluginErrorProp = _var.AddProperty<string>();

            PluginsNames = _var.AddProperty<List<PluginNameAndGroup>>();
            Plugins = env.LocalAgentVM.PluginList;

            Plugins.CollectionChanged += (_, _) => UploadPluginNamesCollection();

            UploadPluginNamesCollection(); // for first initialization

            SelectedPluginInfo = new Property<string>();
            SelectedPlugin = new Property<AlgoPluginViewModel>().AddPostTrigger(plugin =>
            {
                SelectedPluginInfo.Value = $"Package path: {plugin?.PackageInfo.Identity.FilePath}";
            });

            SelectedPluginVersions = new Property<List<AlgoPluginViewModel>>();
            SelectedPluginName = new Property<PluginNameAndGroup>().AddPostTrigger(plugin =>
            {
                if (plugin == null)
                    return;

                SelectedPluginVersions.Value = Plugins?.Where(u => u.DisplayName == plugin.DisplayName).OrderByDescending(u => u.Descriptor.Version).ToList();
                SelectedPlugin.Value = SelectedPluginVersions.Value?.FirstOrDefault();
            });

            IsPluginSelected = SelectedPlugin.Var.IsNotNull();
            IsTradeBotSelected = SelectedPlugin.Var.Check(p => p != null && p.Descriptor.IsTradeBot);
            //IsRunning = ActionOverlay.IsRunning;
            //IsStopping = ActionOverlay.IsCancelling;
            _isPluginValid = PluginErrorProp.Var.IsNull();
            IsSetupValid = !IsUpdatingRange.Var & IsPluginSelected & _allSymbolsValid.Var & _isDateRangeValid.Var & _isPluginValid;
            CanSetup = !isRunning & client.IsConnected;
            //CanStop = ActionOverlay.CanCancel;
            //CanSave = !IsRunning & _hasDataToSave.Var;

            TradeSettingsSummary = _var.AddProperty<string>();

            _mainSymbolToken = SpecialSymbols.MainSymbolPlaceholder;
            //var predefinedSymbolTokens = new VarList<ISymbolInfo>(new ISymbolInfo[] { _mainSymbolToken });
            var predefinedSymbolTokens = new VarDictionary<SymbolKey, ISetupSymbolInfo>();
            predefinedSymbolTokens.Add(_mainSymbolToken.GetKey(), _mainSymbolToken);

            var existingSymbolTokens = _catalog.AllSymbols.Select(s => s.ToKey()).ToList();
            existingSymbolTokens.AddRange(predefinedSymbolTokens.Values);

            // _symbolTokens = VarCollection.Combine(predefinedSymbolTokens, existingSymbolTokens);

            var sortedSymbolTokens = existingSymbolTokens.OrderBy(u => u, new SetupSymbolInfoComparer()).ToList();
            _observableSymbolTokens = sortedSymbolTokens.AsReadOnly();

            Modes = new List<OptionalItem<BacktesterMode>>
            {
                new OptionalItem<BacktesterMode>(BacktesterMode.Backtesting),
                new OptionalItem<BacktesterMode>(BacktesterMode.Visualization),
                new OptionalItem<BacktesterMode>(BacktesterMode.Optimization)
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
                    var plugin = env.LocalAgent.Plugins.GetOrDefault(a.New.Key);
                    UpdateOptimizationState(plugin.Descriptor_);
                    UpdatePluginState(plugin.Descriptor_);
                    PluginConfig = null;
                    PluginSelected?.Invoke();
                    CloseSetupDialog();
                }
                else
                    PluginConfig = null;
            });

            _var.TriggerOnChange(MainSymbolSetup.SelectedTimeframe, a =>
            {
                var data = EnumHelper.AllValues<Feed.Types.Timeframe>().Where(t => t <= a.New).ToList();
                data.Add(Feed.Types.Timeframe.Ticks);
                AvailableModels.Value = data;

                if (_openedPluginSetup != null)
                    _openedPluginSetup.Setup.SelectedTimeFrame = a.New.ToApi();

                if (SelectedModel.Value > a.New && SelectedModel.Value != Feed.Types.Timeframe.Ticks)
                    SelectedModel.Value = a.New;
            });

            _var.TriggerOnChange(MainSymbolSetup.SelectedSymbol, a =>
            {
                if (a.New != null)
                {
                    _mainSymbolToken.Id = a.New.Name;

                    if (_openedPluginSetup != null)
                        _openedPluginSetup.Setup.MainSymbol = a.New.Key.ToKey();

                    MainSymbolShadowSetup.SelectedSymbolName.Value = a.New.Name;
                }
            });

            client.Connected += () =>
            {
                FeedSymbols.ForEach(s => s.Reset());
            };

            UpdateTradeSummary();
        }

        private void UploadPluginNamesCollection()
        {
            PluginsNames.Value = Plugins.DistinctBy(u => u.DisplayName).Select(u => new PluginNameAndGroup(u.DisplayName, u.CurrentGroup)).ToList();
        }

        public BoolVar CanSetup { get; }
        public BoolVar IsSetupValid { get; }
        public BacktesterSettings Settings { get; private set; } = new BacktesterSettings();

        public IObservableList<AlgoPluginViewModel> Plugins { get; }

        public Property<List<PluginNameAndGroup>> PluginsNames { get; }

        public Property<List<AlgoPluginViewModel>> SelectedPluginVersions { get; }

        public Property<List<Feed.Types.Timeframe>> AvailableModels { get; private set; }
        public Property<Feed.Types.Timeframe> SelectedModel { get; private set; }
        public Property<AlgoPluginViewModel> SelectedPlugin { get; private set; }
        public Property<PluginNameAndGroup> SelectedPluginName { get; }
        public Property<string> SelectedPluginInfo { get; }
        public Property<string> PluginErrorProp { get; }
        public BacktesterSymbolSetupViewModel MainSymbolSetup { get; private set; }
        public BacktesterSymbolSetupViewModel MainSymbolShadowSetup { get; private set; }
        public PluginConfig PluginConfig { get; private set; }
        public Property<string> TradeSettingsSummary { get; private set; }
        public List<OptionalItem<BacktesterMode>> Modes { get; }
        public Property<OptionalItem<BacktesterMode>> ModeProp { get; private set; }
        public BacktesterMode Mode => ModeProp.Value.Value;
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

        public async Task PrecacheData(IActionObserver observer, DateTime from, DateTime to)
        {
            await MainSymbolSetup.PrecacheData(observer, from, to, SelectedModel.Value);

            foreach (var symbolSetup in FeedSymbols)
                await symbolSetup.PrecacheData(observer, from, to);
        }

        public void InitToken()
        {
            var mainSymbol = MainSymbolSetup.SelectedSymbol.Value;
            _mainSymbolToken.Id = mainSymbol.Name;
        }

        public void Apply(BacktesterConfig config)
        {
            config.Core.Mode = Mode;
            config.Core.EmulateFrom = DateTime.SpecifyKind(DateRange.From, DateTimeKind.Utc);
            config.Core.EmulateTo = DateTime.SpecifyKind(DateRange.To, DateTimeKind.Utc);
            Settings.Apply(config);

            var selectedPlugin = SelectedPlugin.Value;
            config.Env.PackagePath = selectedPlugin.PackageInfo.Identity.FilePath;
            config.SetPluginConfig(PluginConfig ?? CreateDefaultPluginConfig());

            config.Core.MainSymbol = MainSymbolSetup.SelectedSymbol.Value.Name;
            config.Core.MainTimeframe = MainSymbolSetup.SelectedTimeframe.Value;
            config.Core.ModelTimeframe = SelectedModel.Value;

            foreach (var symbolSetup in FeedSymbols)
            {
                var smbData = symbolSetup.SelectedSymbol.Value;
                var symbolName = smbData.Name;
                var smbKey = new FeedCacheKey(symbolName, symbolSetup.SelectedTimeframe.Value, smbData.Origin).FullInfo;

                if (config.Core.FeedConfig.ContainsKey(symbolName))
                    throw new ArgumentException("Duplicate symbol");

                config.Core.FeedConfig.Add(symbolName, smbKey);
                config.TradeServer.Symbols.Add(symbolName, CustomSymbolInfo.ToData(smbData.Info));
            }

            foreach (var currency in _client.Currencies.Snapshot)
            {
                var c = CustomCurrency.FromAlgo(currency.Value);
                config.TradeServer.Currencies.Add(c.Name, c);
            }
        }

        public BacktesterConfig CreateConfig()
        {
            var config = new BacktesterConfig();
            Apply(config);
            config.Env.FeedCachePath = _catalog.OnlineCollection.StorageFolder;
            config.Env.CustomFeedCachePath = _catalog.CustomCollection.StorageFolder;
            config.Env.WorkingFolderPath = EnvService.Instance.AlgoWorkingFolder;
            return config;
        }

        public async Task LoadConfig(BacktesterConfig config)
        {
            ModeProp.Value = Modes.First(m => m.Value == config.Core.Mode);
            Settings.Load(config);
            UpdateTradeSummary();

            var pluginConfig = config.PluginConfig;
            var plugin = Plugins.FirstOrDefault(p => p.Key.Equals(pluginConfig.Key));
            var selectedPluginName = new PluginNameAndGroup(plugin.DisplayName, plugin.CurrentGroup);

            SelectedPluginName.Value = selectedPluginName;
            SelectedPlugin.Value = plugin;
            PluginConfig = pluginConfig;

            SelectedModel.Value = config.Core.ModelTimeframe;
            var mainSymbolName = config.Core.MainSymbol;
            var mainSymbolCfg = config.Core.FeedConfig[mainSymbolName];
            if (!FeedCacheKey.TryParse(mainSymbolCfg, out var mainSymbolKey))
                throw new ArgumentException($"Failed to parse symbol key '{mainSymbolCfg}'");
            FeedSymbols.Clear();
            FeedSymbols.Add(MainSymbolShadowSetup);
            var smbData = _catalog[new StorageSymbolKey(mainSymbolKey.Symbol, mainSymbolKey.Origin)];
            MainSymbolSetup.SelectedSymbolName.Value = smbData.Name;
            MainSymbolSetup.SelectedSymbol.Value = smbData;
            MainSymbolSetup.SelectedTimeframe.Value = config.Core.MainTimeframe;
            MainSymbolShadowSetup.SelectedTimeframe.Value = mainSymbolKey.TimeFrame;

            foreach (var pair in config.Core.FeedConfig)
            {
                if (pair.Key == mainSymbolName)
                    continue;

                if (!FeedCacheKey.TryParse(pair.Value, out var symbolKey))
                    throw new ArgumentException($"Failed to parse symbol key '{pair.Value}'");

                var smbSetup = CreateSymbolSetupModel(SymbolSetupType.Additional);
                smbData = _catalog[new StorageSymbolKey(symbolKey.Symbol, symbolKey.Origin)];
                smbSetup.SelectedSymbolName.Value = smbData.Name;
                smbSetup.SelectedSymbol.Value = smbData;
                smbSetup.SelectedTimeframe.Value = symbolKey.TimeFrame;
                FeedSymbols.Add(smbSetup);
            }

            // Wait until available range for all symbols are updated
            while (FeedSymbols.Any(s => s.IsUpdating.Value))
            {
                await Task.Delay(100);
            }
            DateRange.From = config.Core.EmulateFrom;
            DateRange.To = config.Core.EmulateTo;
        }


        private PluginConfig CreateDefaultPluginConfig()
        {
            var setup = new BacktesterPluginSetupViewModel(_env.LocalAgent, SelectedPlugin.Value.PluginInfo, this, this.GetSetupContextInfo(), GetClientAccountMetadata());
            setup.Setup.SelectedModel.Value = SelectedModel.Value.ToApi();
            setup.Setup.MainSymbol = MainSymbolSetup.SelectedSymbol.Value.Key.ToKey();
            setup.Setup.SelectedTimeFrame = MainSymbolSetup.SelectedTimeframe.Value.ToApi();
            return setup.GetConfig();
        }

        private AccountMetadataInfo GetClientAccountMetadata()
        {
            var client = _client;
            var accountMetadata = new AccountMetadataInfo(AccountId.Pack(client.Connection.CurrentServer, client.Connection.CurrentLogin),
                client.SortedSymbols.Select(s => s.ToInfo()).ToList(), client.Cache.GetDefaultSymbol().ToInfo());
            return accountMetadata;
        }


        #region Plugin Setup

        private const string SetupWndKey = "SetupAuxWnd";

        public void OpenPluginSetup()
        {
            _localWnd.OpenOrActivateWindow(SetupWndKey, () =>
            {
                _openedPluginSetup = PluginConfig == null
                    ? new BacktesterPluginSetupViewModel(_env.LocalAgent, SelectedPlugin.Value.PluginInfo, this, this.GetSetupContextInfo(), GetClientAccountMetadata())
                    : new BacktesterPluginSetupViewModel(_env.LocalAgent, SelectedPlugin.Value.PluginInfo, this, this.GetSetupContextInfo(), GetClientAccountMetadata(), PluginConfig);
                //_localWnd.OpenMdiWindow(wndKey, _openedPluginSetup);
                _openedPluginSetup.Setup.SelectedModel.Value = SelectedModel.Value.ToApi();
                _openedPluginSetup.Setup.MainSymbol = MainSymbolSetup.SelectedSymbol.Value.Key.ToKey();
                _openedPluginSetup.Setup.SelectedTimeFrame = MainSymbolSetup.SelectedTimeframe.Value.ToApi();
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
            MainSymbolSetup.SelectedSymbol.Value = _catalog[config.MainSymbol];
            MainSymbolSetup.SelectedSymbolName.Value = MainSymbolSetup.SelectedSymbol.Value.Name;
            MainSymbolSetup.SelectedTimeframe.Value = config.SelectedTimeFrame.ToServer();
        }

        #endregion

        private void AddSymbol()
        {
            FeedSymbols.Add(CreateSymbolSetupModel(SymbolSetupType.Additional));
            UpdateSymbolsState();
        }

        private BacktesterSymbolSetupViewModel CreateSymbolSetupModel(SymbolSetupType type, Var<ISymbolData> symbolSrc = null)
        {
            var smb = new BacktesterSymbolSetupViewModel(type, _catalog, symbolSrc);
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

        public override Task<bool> CanCloseAsync(CancellationToken cancellationToken = default)
        {
            _var.Dispose();
            return base.CanCloseAsync(cancellationToken);
        }

        //public override void TryClose(bool? dialogResult = null)
        //{
        //    base.TryClose(dialogResult);

        //    _var.Dispose();
        //}

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

        string IPluginIdProvider.GeneratePluginId(PluginDescriptor descriptor) => descriptor.DisplayName;

        bool IPluginIdProvider.IsValidPluginId(Metadata.Types.PluginType pluginType, string pluginId) => true;

        void IPluginIdProvider.RegisterPluginId(string pluginId) { }

        void IPluginIdProvider.UnregisterPluginId(string pluginId) { }

        #endregion IPluginIdProvider

        #region IAlgoSetupContext

        Feed.Types.Timeframe IAlgoSetupContext.DefaultTimeFrame => MainSymbolSetup.SelectedTimeframe.Value;

        ISetupSymbolInfo IAlgoSetupContext.DefaultSymbol => _mainSymbolToken;

        MappingKey IAlgoSetupContext.DefaultMapping => MappingDefaults.DefaultBarToBarMapping.Key;

        #endregion IAlgoSetupContext
    }

    public class OptionalItem<T> : PropertyChangedBase
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
}
