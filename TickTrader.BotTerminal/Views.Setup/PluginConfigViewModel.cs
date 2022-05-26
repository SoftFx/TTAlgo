using Caliburn.Micro;
using Machinarium.Var;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using TickTrader.Algo.Core.Setup;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Package;
using TickTrader.Algo.Server.PublicAPI.Converters;
using AlgoApi = TickTrader.Algo.Server.PublicAPI;

namespace TickTrader.BotTerminal
{
    public enum PluginSetupMode
    {
        New,
        Edit,
    }


    public sealed class PluginConfigViewModel : PropertyChangedBase
    {
        private readonly VarContext _var = new VarContext();

        private List<PropertySetupViewModel> _allProperties;
        private List<ParameterSetupViewModel> _parameters;
        private List<InputSetupViewModel> _barBasedInputs;
        private List<OutputSetupViewModel> _outputs;
        private AlgoApi.Feed.Types.Timeframe _selectedTimeFrame;
        private StorageSymbolKey _mainSymbol;
        private MappingInfo _selectedMapping;
        private string _instanceId;
        private IPluginIdProvider _idProvider;
        private bool _allowTrade;
        private bool _isolate;
        private bool _visible;
        private bool _runBot;

        public IEnumerable<AlgoApi.Feed.Types.Timeframe> AvailableTimeFrames { get; private set; }

        public bool IsFixedFeed { get; set; }
        public bool IsEmulation { get; set; }

        public bool EnableFeedSetup => !IsFixedFeed && (Descriptor.SetupMainSymbol || !IsBot);

        public bool Visible
        {
            get => _visible;

            set
            {
                if (_visible == value)
                    return;

                _visible = value;
                NotifyOfPropertyChange(nameof(Visible));
            }
        }

        public AlgoApi.Feed.Types.Timeframe SelectedTimeFrame
        {
            get { return _selectedTimeFrame; }
            set
            {
                if (_selectedTimeFrame == value)
                    return;

                var changeInputs = _selectedTimeFrame == AlgoApi.Feed.Types.Timeframe.Ticks || value == AlgoApi.Feed.Types.Timeframe.Ticks;

                _selectedTimeFrame = value;
                NotifyOfPropertyChange(nameof(SelectedTimeFrame));

                if (changeInputs)
                {
                    NotifyOfPropertyChange(nameof(Inputs));
                    NotifyOfPropertyChange(nameof(HasInputs));
                }

                AvailableModels.Value = SetupMetadata.Api.TimeFrames.Select(u => u.ToApi()).Where(t => t <= value).OrderBy(u => (int)u).ToList();

                if (SelectedModel.Value > value)
                    SelectedModel.Value = value;
            }
        }

        public IReadOnlyList<StorageSymbolKey> AvailableSymbols { get; private set; }

        public StorageSymbolKey MainSymbol
        {
            get { return _mainSymbol; }
            set
            {
                if (_mainSymbol == value)
                    return;

                _mainSymbol = value;

                NotifyOfPropertyChange(nameof(MainSymbol));
            }
        }

        public IReadOnlyList<MappingInfo> AvailableMappings { get; private set; }

        public MappingInfo SelectedMapping
        {
            get { return _selectedMapping; }
            set
            {
                if (_selectedMapping == value)
                    return;

                _selectedMapping = value;
                NotifyOfPropertyChange(nameof(SelectedMapping));
            }
        }

        public Property<List<AlgoApi.Feed.Types.Timeframe>> AvailableModels { get; private set; }

        public Property<AlgoApi.Feed.Types.Timeframe> SelectedModel { get; private set; }

        public IEnumerable<ParameterSetupViewModel> Parameters => _parameters;

        public IEnumerable<InputSetupViewModel> Inputs => ActiveInputs;

        public IEnumerable<OutputSetupViewModel> Outputs => _outputs;

        public bool HasInputsOrParams => HasParams || HasInputs;

        public bool HasParams => _parameters.Count > 0;

        public bool HasInputs => ActiveInputs.Count > 0;

        public bool HasOutputs => _outputs.Count > 0;

        public bool HasDescription => !string.IsNullOrWhiteSpace(Descriptor?.Description);

        public PluginDescriptor Descriptor { get; }

        public PluginInfo Plugin { get; }

        public bool IsValid { get; private set; }

        public bool IsEmpty { get; private set; }

        public SetupMetadata SetupMetadata { get; }

        public PluginSetupMode Mode { get; }

        public bool IsEditMode => Mode == PluginSetupMode.Edit;

        public bool CanBeSkipped => IsEmpty && Descriptor.IsValid && !Descriptor.IsTradeBot;

        public bool IsBot => Descriptor.IsTradeBot;

        public string InstanceId
        {
            get { return _instanceId; }
            set
            {
                if (_instanceId == value)
                    return;

                _instanceId = value;
                NotifyOfPropertyChange(nameof(InstanceId));
                NotifyOfPropertyChange(nameof(IsInstanceIdValid));
                Validate();
            }
        }

        public bool IsInstanceIdValid => Mode == PluginSetupMode.Edit || _idProvider.IsValidPluginId(Descriptor.Type, InstanceId);

        public bool AllowTrade
        {
            get { return _allowTrade; }
            set
            {
                if (_allowTrade == value)
                    return;

                _allowTrade = value;
                NotifyOfPropertyChange(nameof(AllowTrade));
            }
        }

        public bool Isolated
        {
            get { return _isolate; }
            set
            {
                if (_isolate == value)
                    return;

                _isolate = value;
                NotifyOfPropertyChange(nameof(Isolated));
            }
        }

        public bool RunBot
        {
            get { return _runBot; }
            set
            {
                if (_runBot == value)
                    return;

                _runBot = value;
                NotifyOfPropertyChange(nameof(RunBot));
            }
        }

        private List<InputSetupViewModel> ActiveInputs => _barBasedInputs;

        public event System.Action ValidityChanged = delegate { };

        public event System.Action<PluginConfigViewModel> ConfigLoaded;

        public PluginConfigViewModel(PluginInfo plugin, SetupMetadata setupMetadata, IPluginIdProvider idProvider, PluginSetupMode mode)
        {
            Plugin = plugin;
            Descriptor = plugin.Descriptor_;
            SetupMetadata = setupMetadata;
            _idProvider = idProvider;
            Mode = mode;
            MainSymbol = setupMetadata.DefaultSymbol.ToKey();
            Visible = true;
            RunBot = true;

            _paramsFileHistory.SetContext(plugin.ToString());

            AvailableModels = _var.AddProperty<List<AlgoApi.Feed.Types.Timeframe>>();
            SelectedModel = _var.AddProperty(AlgoApi.Feed.Types.Timeframe.M1);

            Init();
        }

        public void Load(PluginConfig cfg, bool onlyParams = false)
        {
            SelectedTimeFrame = cfg.Timeframe.ToApi();
            SelectedModel.Value = cfg.ModelTimeframe.ToApi();
            MainSymbol = AvailableSymbols.GetSymbolOrDefault(cfg.MainSymbol.ToKey()) ?? AvailableSymbols.GetSymbolOrAny(SetupMetadata.DefaultSymbol.ToKey());

            if (!IsEmulation)
            {
                SelectedMapping = SetupMetadata.Mappings.GetBarToBarMappingOrDefault(cfg.SelectedMapping);

                if (!onlyParams)
                    InstanceId = cfg.InstanceId;

                AllowTrade = cfg.Permissions.TradeAllowed;
                Isolated = cfg.Permissions.Isolated;
            }

            foreach (var scrProperty in cfg.UnpackProperties())
            {
                var thisProperty = _allProperties.FirstOrDefault(p => p.Id == scrProperty.PropertyId);
                if (thisProperty != null)
                    thisProperty.Load(scrProperty);
            }

            ConfigLoaded?.Invoke(this);
        }

        public PluginConfig Save()
        {
            var cfg = new PluginConfig
            {
                Timeframe = SelectedTimeFrame.ToServer(),
                ModelTimeframe = SelectedModel.Value.ToServer(),
                MainSymbol = MainSymbol.ToConfig(),
                SelectedMapping = SelectedMapping.Key,
                InstanceId = InstanceId,
                Permissions = new PluginPermissions { TradeAllowed = _allowTrade, Isolated = _isolate }
            };

            cfg.PackProperties(_allProperties.Select(p => p.Save()));
            return cfg;
        }

        public void Reset()
        {
            SelectedModel.Value = AlgoApi.Feed.Types.Timeframe.Ticks;
            SelectedTimeFrame = SetupMetadata.Context.DefaultTimeFrame.ToApi();
            MainSymbol = (StorageSymbolKey)AvailableSymbols.GetSymbolOrAny(SetupMetadata.DefaultSymbol.ToKey());
            SelectedMapping = SetupMetadata.Mappings.GetBarToBarMappingOrDefault(SetupMetadata.Context.DefaultMapping);
            InstanceId = _idProvider.GeneratePluginId(Descriptor);

            AllowTrade = true;
            Isolated = true;

            foreach (var p in _allProperties)
                p.Reset();
        }

        public void Validate()
        {
            IsValid = CheckValidity();
            ValidityChanged();
        }

        #region Load & save parameters

        private const string ParamsFileFilter = "Param files (*.apr)|*.apr";
        private static readonly FileHistory _paramsFileHistory = new FileHistory();

        public Var<ObservableCollection<FileHistory.Entry>> ConfigLoadHistory => _paramsFileHistory.Items;

        public IEnumerable<IResult> SaveParams()
        {
            var dialog = new SaveFileDialog();
            dialog.FileName = Descriptor.DisplayName + ".apr";
            dialog.Filter = ParamsFileFilter;

            var showAction = VmActions.ShowWin32Dialog(dialog);
            yield return showAction;

            if (showAction.Result == true)
            {
                Exception saveError = null;

                try
                {
                    var config = Save();
                    config.Key = Plugin.Key;
                    Algo.Core.Config.PluginConfig.FromDomain(config).SaveToFile(dialog.FileName);
                }
                catch (Exception ex)
                {
                    saveError = ex;
                }

                if (saveError != null)
                    yield return VmActions.ShowError("Failed to save parameters: " + saveError.Message, "Error");
            }
        }

        public IEnumerable<IResult> LoadParams()
        {
            OpenFileDialog dialog = new OpenFileDialog();
            //    var wnd = Window.GetWindow(this);
            dialog.Filter = ParamsFileFilter;
            dialog.CheckFileExists = true;

            var showAction = VmActions.ShowWin32Dialog(dialog);
            yield return showAction;

            if (showAction.Result == true)
            {
                var ex = LoadParamsFromFile(dialog.FileName);
                if (ex != null)
                    yield return VmActions.ShowError("Failed to load parameters: " + ex.Message);
            }
        }

        public IResult LoadParamsFrom(FileHistory.Entry historyItem)
        {
            var ex = LoadParamsFromFile(historyItem.FullPath);

            if (ex != null)
            {
                _paramsFileHistory.Remove(historyItem);
                return VmActions.ShowError("Failed to load parameters: " + ex.Message);
            }

            return null;
        }

        private Exception LoadParamsFromFile(string filePath)
        {
            PluginConfig cfg = null;

            try
            {
                var ext = Path.GetExtension(filePath);
                cfg = Algo.Core.Config.PluginConfig.LoadFromFile(filePath).ToDomain();
                _paramsFileHistory.Add(filePath, true);

                if (cfg != null)
                {
                    if (cfg.Key != null && cfg.Key.DescriptorId != Plugin.Key.DescriptorId)
                        return new AlgoException($"Loaded config descriptorId '{cfg.Key.DescriptorId}' doesn't match current plugin descriptorId '{Plugin.Key.DescriptorId}'");
                    Load(cfg, true);
                }

                return null;
            }
            catch (Exception ex)
            {
                return ex;
            }
        }

        #endregion

        private bool CheckValidity()
        {
            return Descriptor.Error == Metadata.Types.MetadataErrorCode.NoMetadataError && _allProperties.All(p => !p.HasError) && IsInstanceIdValid;
        }

        private void Init()
        {
            AvailableTimeFrames = SetupMetadata.Api.TimeFrames.Where(t => t != Feed.Types.Timeframe.Ticks).Select(u => u.ToApi());
            AvailableSymbols = SetupMetadata.Account.GetAvaliableSymbols(SetupMetadata.Context.DefaultSymbol.ToKey()).Where(u => u.Origin != SymbolConfig.Types.SymbolOrigin.Token).ToList();
            AvailableMappings = SetupMetadata.Mappings.BarToBarMappings;


            _parameters = Descriptor.Parameters.Select(CreateParameter).ToList();
            _barBasedInputs = Descriptor.Inputs.Select(CreateBarBasedInput).ToList();
            _outputs = Descriptor.Outputs.Select(CreateOutput).ToList();

            _allProperties = _parameters.Concat<PropertySetupViewModel>(_barBasedInputs).Concat(_outputs).ToList();
            _allProperties.ForEach(p => p.ErrorChanged += s => Validate());

            IsEmpty = _allProperties.Count == 0 && !Descriptor.SetupMainSymbol;

            Reset();
            Validate();
        }

        private ParameterSetupViewModel CreateParameter(ParameterDescriptor descriptor)
        {
            if (!descriptor.IsValid)
                return new ParameterSetupViewModel.Invalid(descriptor);

            if (descriptor.IsEnum)
                return new EnumParamSetupViewModel(descriptor);
            if (descriptor.DataType == ParameterSetupViewModel.NullableIntTypeName)
                return new NullableIntParamSetupViewModel(descriptor);
            if (descriptor.DataType == ParameterSetupViewModel.NullableDoubleTypeName)
                return new NullableDoubleParamSetupViewModel(descriptor);

            switch (descriptor.DataType)
            {
                case "System.Boolean": return new BoolParamSetupViewModel(descriptor);
                case "System.Int32": return new IntParamSetupViewModel(descriptor);
                case "System.Double": return new DoubleParamSetupViewModel(descriptor);
                case "System.String": return new StringParamSetupViewModel(descriptor);
                case "TickTrader.Algo.Api.File": return new FileParamSetupViewModel(descriptor);
                default: return new ParameterSetupViewModel.Invalid(descriptor, ErrorMsgCodes.UnsupportedParameterType);
            }
        }

        private InputSetupViewModel CreateBarBasedInput(InputDescriptor descriptor)
        {
            if (!descriptor.IsValid)
                return new InputSetupViewModel.Invalid(descriptor);

            switch (descriptor.DataSeriesBaseTypeFullName)
            {
                case "System.Double": return new BarToDoubleInputSetupViewModel(descriptor, SetupMetadata);
                case "TickTrader.Algo.Api.Bar": return new BarToBarInputSetupViewModel(descriptor, SetupMetadata);
                default: return new InputSetupViewModel.Invalid(descriptor, ErrorMsgCodes.UnsupportedInputType);
            }
        }

        private OutputSetupViewModel CreateOutput(OutputDescriptor descriptor)
        {
            if (!descriptor.IsValid)
                return new OutputSetupViewModel.Invalid(descriptor);

            switch (descriptor.DataSeriesBaseTypeFullName)
            {
                case "System.Double": return new ColoredLineOutputSetupViewModel(descriptor);
                case "TickTrader.Algo.Api.Marker": return new MarkerSeriesOutputSetupViewModel(descriptor);
                default: return new OutputSetupViewModel.Invalid(descriptor, ErrorMsgCodes.UnsupportedOutputType);
            }
        }
    }
}
