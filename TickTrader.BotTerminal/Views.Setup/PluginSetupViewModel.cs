using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TickTrader.Algo.Api;
using TickTrader.Algo.Common.Info;
using TickTrader.Algo.Common.Model.Config;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Metadata;

namespace TickTrader.BotTerminal
{
    public enum PluginSetupMode
    {
        New,
        Edit,
    }


    public abstract class PluginSetupViewModel : PropertyChangedBase
    {
        private List<PropertySetupViewModel> _allProperties;
        private List<ParameterSetupViewModel> _parameters;
        private List<InputSetupViewModel> _barBasedInputs;
        private List<InputSetupViewModel> _tickBasedInputs;
        private List<OutputSetupViewModel> _outputs;
        private TimeFrames _selectedTimeFrame;
        private SymbolInfo _mainSymbol;
        private SymbolMapping _selectedMapping;
        private string _instanceId;
        private PluginPermissions _permissions;


        public IEnumerable<TimeFrames> AvailableTimeFrames { get; private set; }

        public abstract bool AllowChangeTimeFrame { get; }

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

        public abstract bool AllowChangeMainSymbol { get; }

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

        public IReadOnlyList<SymbolMapping> AvailableMappings { get; private set; }

        public abstract bool AllowChangeMapping { get; }

        public SymbolMapping SelectedMapping
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

        public bool HasDescription => !string.IsNullOrWhiteSpace(Metadata?.Description);

        public PluginMetadataInfo Metadata { get; }

        public PluginInfo Plugin { get; }

        public bool IsValid { get; private set; }

        public bool IsEmpty { get; private set; }

        public SetupMetadataInfo SetupMetadata { get; }

        public SetupContextInfo SetupContext { get; }

        public PluginSetupMode Mode { get; }

        public bool IsEditMode => Mode == PluginSetupMode.Edit;

        public bool CanBeSkipped => IsEmpty && Metadata.IsValid && Metadata.Type != AlgoTypes.Robot;

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

        public bool IsInstanceIdValid => Mode == PluginSetupMode.Edit ? true : SetupMetadata.IdProvider.IsValidPluginId(Metadata, InstanceId);

        public PluginPermissions Permissions
        {
            get { return _permissions; }
            set
            {
                if (_permissions == value)
                    return;

                _permissions = value;
                NotifyOfPropertyChange(nameof(Permissions));
                NotifyOfPropertyChange(nameof(AllowTrade));
                NotifyOfPropertyChange(nameof(Isolated));
            }
        }

        public bool AllowTrade
        {
            get { return Permissions.TradeAllowed; }
            set
            {
                if (Permissions.TradeAllowed == value)
                    return;

                Permissions.TradeAllowed = value;
                NotifyOfPropertyChange(nameof(AllowTrade));
            }
        }

        public bool Isolated
        {
            get { return Permissions.Isolated; }
            set
            {
                if (Permissions.Isolated == value)
                    return;

                Permissions.Isolated = value;
                NotifyOfPropertyChange(nameof(Isolated));
            }
        }


        private List<InputSetupViewModel> ActiveInputs => _selectedTimeFrame == TimeFrames.Ticks ? _tickBasedInputs : _barBasedInputs;


        public event System.Action ValidityChanged = delegate { };


        public PluginSetupViewModel(PluginInfo plugin, SetupMetadataInfo setupMetadata, SetupContextInfo setupContext, PluginSetupMode mode)
        {
            Plugin = plugin;
            Metadata = plugin.Metadata;
            SetupMetadata = setupMetadata;
            Mode = mode;
        }


        public virtual void Load(PluginConfig cfg)
        {
            SelectedTimeFrame = cfg.TimeFrame;
            MainSymbol = AvailableSymbols.GetSymbolOrAny(cfg.MainSymbol);
            SelectedMapping = SetupMetadata.SymbolMappings.GetBarToBarMappingOrDefault(cfg.SelectedMapping);
            InstanceId = cfg.InstanceId;
            Permissions = cfg.Permissions.Clone();
            foreach (var scrProperty in cfg.Properties)
            {
                var thisProperty = _allProperties.FirstOrDefault(p => p.Id == scrProperty.Id);
                if (thisProperty != null)
                    thisProperty.Load(scrProperty);
            }
        }

        public virtual PluginConfig Save()
        {
            var cfg = SaveToConfig();
            cfg.TimeFrame = SelectedTimeFrame;
            cfg.MainSymbol = MainSymbol.Name;
            cfg.SelectedMapping = SelectedMapping.Name;
            cfg.InstanceId = InstanceId;
            cfg.Permissions = Permissions.Clone();
            foreach (var property in _allProperties)
                cfg.Properties.Add(property.Save());
            return cfg;
        }

        public virtual void Reset()
        {
            SelectedTimeFrame = SetupContext.DefaultTimeFrame;
            MainSymbol = AvailableSymbols.GetSymbolOrAny(SetupContext.DefaultSymbolCode);
            SelectedMapping = SetupMetadata.SymbolMappings.GetBarToBarMappingOrDefault(SetupContext.DefaultMapping);
            InstanceId = SetupMetadata.IdProvider.GeneratePluginId(Metadata);

            _parameters.ForEach(p => p.Reset());
            foreach (var p in _allProperties)
                p.Reset();
        }

        public void Validate()
        {
            IsValid = CheckValidity();
            ValidityChanged();
        }


        protected abstract PluginConfig SaveToConfig();


        protected virtual bool CheckValidity()
        {
            return Metadata.Error == null && _allProperties.All(p => !p.HasError) && IsInstanceIdValid;
        }


        protected void Init()
        {
            AvailableTimeFrames = Enum.GetValues(typeof(TimeFrames)).Cast<TimeFrames>().Where(tf => tf != TimeFrames.TicksLevel2).ToList();
            AvailableSymbols = SetupMetadata.Account.GetAvaliableSymbols(SetupMetadata.Context.DefaultSymbolCode);
            AvailableMappings = SetupMetadata.SymbolMappings.BarToBarMappings;

            _parameters = Metadata.Parameters.Select(CreateParameter).ToList();
            _barBasedInputs = Metadata.Inputs.Select(CreateBarBasedInput).ToList();
            _tickBasedInputs = Metadata.Inputs.Select(CreateTickBasedInput).ToList();
            _outputs = Metadata.Outputs.Select(CreateOutput).ToList();

            _allProperties = _parameters.Concat<PropertySetupViewModel>(_barBasedInputs).Concat(_tickBasedInputs).Concat(_outputs).ToList();
            _allProperties.ForEach(p => p.ErrorChanged += s => Validate());

            IsEmpty = _allProperties.Count == 0 && !Metadata.SetupMainSymbol;

            Reset();
            Validate();
        }


        private ParameterSetupViewModel CreateParameter(ParameterMetadataInfo metadata)
        {
            if (!metadata.IsValid)
                return new ParameterSetupViewModel.Invalid(metadata);

            if (metadata.IsEnum)
                return new EnumParamSetupViewModel(metadata);
            if (metadata.DataType == ParameterSetupViewModel.NullableIntTypeName)
                return new NullableIntParamSetupModel(metadata);
            if (metadata.DataType == ParameterSetupViewModel.NullableDoubleTypeName)
                return new NullableDoubleParamSetupModel(metadata);

            switch (metadata.DataType)
            {
                case "System.Boolean": return new BoolParamSetupModel(metadata);
                case "System.Int32": return new IntParamSetupModel(metadata);
                case "System.Double": return new DoubleParamSetupModel(metadata);
                case "System.String": return new StringParamSetupModel(metadata);
                case "TickTrader.Algo.Api.File": return new FileParamSetupViewModel(metadata);
                default: return new ParameterSetupViewModel.Invalid(metadata, ErrorMsgCodes.UnsupportedParameterType);
            }
        }

        private InputSetupViewModel CreateBarBasedInput(InputMetadataInfo metadata)
        {
            if (!metadata.IsValid)
                return new InputSetupViewModel.Invalid(metadata);

            switch (metadata.DataSeriesBaseTypeFullName)
            {
                case "System.Double": return new BarToDoubleInputSetupModel(metadata, SetupMetadata, Context.DefaultSymbolCode, $"{Context.DefaultMapping}.Close");
                case "TickTrader.Algo.Api.Bar": return new BarToBarInputSetupViewModel(metadata, SetupMetadata, Context.DefaultSymbolCode, Context.DefaultMapping);
                //case "TickTrader.Algo.Api.Quote": return new QuoteInputSetupModel(descriptor, Metadata, DefaultSymbolCode, false);
                //case "TickTrader.Algo.Api.QuoteL2": return new QuoteInputSetupModel(descriptor, Metadata, DefaultSymbolCode, true);
                default: return new InputSetupViewModel.Invalid(metadata, ErrorMsgCodes.UnsupportedInputType);
            }
        }

        private InputSetupViewModel CreateTickBasedInput(InputMetadataInfo metadata)
        {
            if (!metadata.IsValid)
                return new InputSetupViewModel.Invalid(metadata);

            switch (metadata.DataSeriesBaseTypeFullName)
            {
                case "System.Double": return new QuoteToDoubleInputSetupModel(metadata, SetupMetadata, Context.DefaultSymbolCode, $"{Context.DefaultMapping}.Close");
                case "TickTrader.Algo.Api.Bar": return new QuoteToBarInputSetupViewModel(metadata, SetupMetadata, Context.DefaultSymbolCode, Context.DefaultMapping);
                case "TickTrader.Algo.Api.Quote": return new QuoteInputSetupViewModel(metadata, SetupMetadata, Context.DefaultSymbolCode, false);
                case "TickTrader.Algo.Api.QuoteL2": return new QuoteInputSetupViewModel(metadata, SetupMetadata, Context.DefaultSymbolCode, true);
                default: return new InputSetupViewModel.Invalid(metadata, ErrorMsgCodes.UnsupportedInputType);
            }
        }

        private OutputSetupViewModel CreateOutput(OutputMetadataInfo descriptor)
        {
            if (!descriptor.IsValid)
                return new OutputSetupViewModel.Invalid(descriptor);

            switch (descriptor.DataSeriesBaseTypeFullName)
            {
                case "System.Double": return new ColoredLineOutputSetupModel(descriptor);
                case "TickTrader.Algo.Api.Marker": return new MarkerSeriesOutputSetupViewModel(descriptor);
                default: return new OutputSetupViewModel.Invalid(descriptor, ErrorMsgCodes.UnsupportedOutputType);
            }
        }
    }
}
