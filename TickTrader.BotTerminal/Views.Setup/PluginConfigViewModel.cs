using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TickTrader.Algo.Api;
using TickTrader.Algo.Common.Info;
using TickTrader.Algo.Common.Model.Config;
using TickTrader.Algo.Common.Model.Setup;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Metadata;

namespace TickTrader.BotTerminal
{
    public enum PluginSetupMode
    {
        New,
        Edit,
    }

    public sealed class PluginConfigViewModel : PropertyChangedBase
    {
        private List<PropertySetupViewModel> _allProperties;
        private List<ParameterSetupViewModel> _parameters;
        private List<InputSetupViewModel> _barBasedInputs;
        private List<InputSetupViewModel> _tickBasedInputs;
        private List<OutputSetupViewModel> _outputs;
        private TimeFrames _selectedTimeFrame;
        private SymbolInfo _mainSymbol;
        private MappingInfo _selectedMapping;
        private string _instanceId;
        private IPluginIdProvider _idProvider;
        private bool _allowTrade;
        private bool _isolate;
        private SymbolInfo _defaultSymbol;

        public IEnumerable<TimeFrames> AvailableTimeFrames { get; private set; }

        public bool IsFixedFeed { get; set; }
        public bool IsEmulation { get; set; }

        public bool EnableFeedSetup => !IsFixedFeed && Descriptor.SetupMainSymbol;

        public TimeFrames SelectedTimeFrame
        {
            get { return _selectedTimeFrame; }
            set
            {
                if (_selectedTimeFrame == value)
                    return;

                var changeInputs = _selectedTimeFrame == TimeFrames.Ticks || value == TimeFrames.Ticks;
                _selectedTimeFrame = value;
                NotifyOfPropertyChange(nameof(SelectedTimeFrame));
                if (changeInputs)
                {
                    NotifyOfPropertyChange(nameof(Inputs));
                    NotifyOfPropertyChange(nameof(HasInputs));
                }
            }
        }

        public IReadOnlyList<SymbolInfo> AvailableSymbols { get; private set; }

        public SymbolInfo MainSymbol
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

        public bool CanBeSkipped => IsEmpty && Descriptor.IsValid && Descriptor.Type != AlgoTypes.Robot;

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

        public bool IsInstanceIdValid => Mode == PluginSetupMode.Edit ? true : _idProvider.IsValidPluginId(Descriptor, InstanceId);

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

        private List<InputSetupViewModel> ActiveInputs => _selectedTimeFrame == TimeFrames.Ticks ? _tickBasedInputs : _barBasedInputs;

        public event System.Action ValidityChanged = delegate { };

        public PluginConfigViewModel(PluginInfo plugin, SetupMetadata setupMetadata, IPluginIdProvider idProvider, PluginSetupMode mode)
        {
            Plugin = plugin;
            Descriptor = plugin.Descriptor;
            SetupMetadata = setupMetadata;
            _idProvider = idProvider;
            Mode = mode;

            Init();
        }

        public void Load(PluginConfig cfg)
        {
            SelectedTimeFrame = cfg.TimeFrame;
            MainSymbol = AvailableSymbols.GetSymbolOrDefault(cfg.MainSymbol)
                ?? AvailableSymbols.GetSymbolOrAny(SetupMetadata.DefaultSymbol);
            SelectedMapping = SetupMetadata.Mappings.GetBarToBarMappingOrDefault(cfg.SelectedMapping);
            InstanceId = cfg.InstanceId;
            AllowTrade = cfg.Permissions.TradeAllowed;
            Isolated = cfg.Permissions.Isolated;
            foreach (var scrProperty in cfg.Properties)
            {
                var thisProperty = _allProperties.FirstOrDefault(p => p.Id == scrProperty.Id);
                if (thisProperty != null)
                    thisProperty.Load(scrProperty);
            }
        }

        public PluginConfig Save()
        {
            var cfg = new PluginConfig();
            cfg.TimeFrame = SelectedTimeFrame;
            cfg.MainSymbol = MainSymbol.ToConfig();
            cfg.SelectedMapping = SelectedMapping.Key;
            cfg.InstanceId = InstanceId;
            cfg.Permissions = new PluginPermissions();
            cfg.Permissions.TradeAllowed = _allowTrade;
            cfg.Permissions.Isolated = _isolate;
            foreach (var property in _allProperties)
                cfg.Properties.Add(property.Save());
            return cfg;
        }

        public void Reset()
        {
            SelectedTimeFrame = SetupMetadata.Context.DefaultTimeFrame;
            MainSymbol = AvailableSymbols.GetSymbolOrAny(SetupMetadata.DefaultSymbol);
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

        private bool CheckValidity()
        {
            return Descriptor.Error == AlgoMetadataErrors.None && _allProperties.All(p => !p.HasError) && IsInstanceIdValid;
        }

        private void Init()
        {
            AvailableTimeFrames = SetupMetadata.Api.TimeFrames;
            AvailableSymbols = SetupMetadata.Account.GetAvaliableSymbols(SetupMetadata.Context.DefaultSymbol);
            AvailableMappings = SetupMetadata.Mappings.BarToBarMappings;


            _parameters = Descriptor.Parameters.Select(CreateParameter).ToList();
            _barBasedInputs = Descriptor.Inputs.Select(CreateBarBasedInput).ToList();
            _tickBasedInputs = Descriptor.Inputs.Select(CreateTickBasedInput).ToList();
            _outputs = Descriptor.Outputs.Select(CreateOutput).ToList();

            _allProperties = _parameters.Concat<PropertySetupViewModel>(_barBasedInputs).Concat(_tickBasedInputs).Concat(_outputs).ToList();
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
                //case "TickTrader.Algo.Api.Quote": return new QuoteInputSetupModel(descriptor, Metadata, DefaultSymbolCode, false);
                //case "TickTrader.Algo.Api.QuoteL2": return new QuoteInputSetupModel(descriptor, Metadata, DefaultSymbolCode, true);
                default: return new InputSetupViewModel.Invalid(descriptor, ErrorMsgCodes.UnsupportedInputType);
            }
        }

        private InputSetupViewModel CreateTickBasedInput(InputDescriptor descriptor)
        {
            if (!descriptor.IsValid)
                return new InputSetupViewModel.Invalid(descriptor);

            switch (descriptor.DataSeriesBaseTypeFullName)
            {
                case "System.Double": return new QuoteToDoubleInputSetupViewModel(descriptor, SetupMetadata);
                case "TickTrader.Algo.Api.Bar": return new QuoteToBarInputSetupViewModel(descriptor, SetupMetadata);
                case "TickTrader.Algo.Api.Quote": return new QuoteInputSetupViewModel(descriptor, SetupMetadata, false);
                case "TickTrader.Algo.Api.QuoteL2": return new QuoteInputSetupViewModel(descriptor, SetupMetadata, true);
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
