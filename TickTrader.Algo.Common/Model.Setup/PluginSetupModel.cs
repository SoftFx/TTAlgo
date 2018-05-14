using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TickTrader.Algo.Api;
using TickTrader.Algo.Common.Lib;
using TickTrader.Algo.Common.Model.Config;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Metadata;
using TickTrader.Algo.Core.Repository;

namespace TickTrader.Algo.Common.Model.Setup
{
    public enum PluginSetupMode
    {
        New,
        Edit,
    }


    public abstract class PluginSetupModel : ObservableObject, ICloneable
    {
        private List<PropertySetupModel> _allProperties;
        private List<ParameterSetupModel> _parameters;
        private List<InputSetupModel> _barBasedInputs;
        private List<InputSetupModel> _tickBasedInputs;
        private List<OutputSetupModel> _outputs;
        private TimeFrames _selectedTimeFrame;
        private ISymbolInfo _mainSymbol;
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
                NotifyPropertyChanged(nameof(SelectedTimeFrame));
                if (changeInputs)
                {
                    NotifyPropertyChanged(nameof(Inputs));
                    NotifyPropertyChanged(nameof(HasInputs));
                }
            }
        }

        public IReadOnlyList<ISymbolInfo> AvailableSymbols { get; private set; }

        public abstract bool AllowChangeMainSymbol { get; }

        public ISymbolInfo MainSymbol
        {
            get { return _mainSymbol; }
            set
            {
                if (_mainSymbol == value)
                    return;

                _mainSymbol = value;
                NotifyPropertyChanged(nameof(MainSymbol));
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
                NotifyPropertyChanged(nameof(SelectedMapping));
            }
        }


        public IEnumerable<ParameterSetupModel> Parameters => _parameters;

        public IEnumerable<InputSetupModel> Inputs => ActiveInputs;

        public IEnumerable<OutputSetupModel> Outputs => _outputs;

        public bool HasInputsOrParams => HasParams || HasInputs;

        public bool HasParams => _parameters.Count > 0;

        public bool HasInputs => ActiveInputs.Count > 0;

        public bool HasOutputs => _outputs.Count > 0;

        public bool HasDescription => !string.IsNullOrWhiteSpace(Descriptor.Descriptor?.Description);

        public PluginMetadata Descriptor { get; }

        public AlgoPluginRef PluginRef { get; }

        public bool IsValid { get; private set; }

        public bool IsEmpty { get; private set; }

        public IAlgoSetupMetadata Metadata { get; }

        public IAlgoSetupContext Context { get; }

        public PluginSetupMode Mode { get; }

        public bool IsEditMode => Mode == PluginSetupMode.Edit;

        public bool CanBeSkipped => IsEmpty && Descriptor.Descriptor.IsValid && Descriptor.Descriptor.Type != AlgoTypes.Robot;

        public string InstanceId
        {
            get { return _instanceId; }
            set
            {
                if (_instanceId == value)
                    return;

                _instanceId = value;
                NotifyPropertyChanged(nameof(InstanceId));
                NotifyPropertyChanged(nameof(IsInstanceIdValid));
                Validate();
            }
        }

        public bool IsInstanceIdValid => Mode == PluginSetupMode.Edit ? true : Metadata.IdProvider.IsValidPluginId(Descriptor.Descriptor, InstanceId);

        public PluginPermissions Permissions
        {
            get { return _permissions; }
            set
            {
                if (_permissions == value)
                    return;

                _permissions = value;
                NotifyPropertyChanged(nameof(Permissions));
                NotifyPropertyChanged(nameof(AllowTrade));
                NotifyPropertyChanged(nameof(Isolated));
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
                NotifyPropertyChanged(nameof(AllowTrade));
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
                NotifyPropertyChanged(nameof(Isolated));
            }
        }


        private List<InputSetupModel> ActiveInputs => _selectedTimeFrame == TimeFrames.Ticks ? _tickBasedInputs : _barBasedInputs;


        public event Action ValidityChanged = delegate { };


        public PluginSetupModel(AlgoPluginRef pRef, IAlgoSetupMetadata metadata, IAlgoSetupContext context, PluginSetupMode mode)
        {
            PluginRef = pRef;
            Descriptor = pRef.Metadata;
            Metadata = metadata;
            Context = context;
            Mode = mode;
        }


        public object Clone()
        {
            return Clone(Mode);
        }

        public abstract object Clone(PluginSetupMode newMode);


        public virtual void Apply(IPluginSetupTarget target)
        {
            _parameters.ForEach(p => p.Apply(target));
            _outputs.ForEach(p => p.Apply(target));
            ActiveInputs.Foreach(p => p.Apply(target));
        }

        public virtual void Load(PluginConfig cfg)
        {
            SelectedTimeFrame = cfg.TimeFrame;
            MainSymbol = AvailableSymbols.GetSymbolOrAny(cfg.MainSymbol);
            SelectedMapping = Metadata.SymbolMappings.GetBarToBarMappingOrDefault(cfg.SelectedMapping);
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

        public void SetWorkingFolder(string workingFolder)
        {
            foreach (FileParamSetupModel fileParam in _parameters.Where(p => p is FileParamSetupModel))
            {
                if (Path.GetFullPath(fileParam.FilePath) != fileParam.FilePath)
                {
                    fileParam.FilePath = Path.Combine(workingFolder, fileParam.FileName);
                }
            }
        }

        public virtual void Reset()
        {
            SelectedTimeFrame = Context.DefaultTimeFrame;
            MainSymbol = AvailableSymbols.GetSymbolOrAny(Context.DefaultSymbolCode);
            SelectedMapping = Metadata.SymbolMappings.GetBarToBarMappingOrDefault(Context.DefaultMapping);
            InstanceId = Metadata.IdProvider.GeneratePluginId(Descriptor.Descriptor);

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
            return Descriptor.Descriptor.IsValid && _allProperties.All(p => !p.HasError) && IsInstanceIdValid;
        }


        protected void Init()
        {
            AvailableTimeFrames = Enum.GetValues(typeof(TimeFrames)).Cast<TimeFrames>().Where(tf => tf != TimeFrames.TicksLevel2).ToList();
            AvailableSymbols = Metadata.GetAvaliableSymbols(Context.DefaultSymbolCode);
            AvailableMappings = Metadata.SymbolMappings.BarToBarMappings;

            _parameters = Descriptor.Parameters.Select(CreateParameter).ToList();
            _barBasedInputs = Descriptor.Inputs.Select(CreateBarBasedInput).ToList();
            _tickBasedInputs = Descriptor.Inputs.Select(CreateTickBasedInput).ToList();
            _outputs = Descriptor.Outputs.Select(CreateOutput).ToList();

            _allProperties = _parameters.Concat<PropertySetupModel>(_barBasedInputs).Concat(_tickBasedInputs).Concat(_outputs).ToList();
            _allProperties.ForEach(p => p.ErrorChanged += s => Validate());

            IsEmpty = _allProperties.Count == 0 && !Descriptor.Descriptor.SetupMainSymbol;

            Reset();
            Validate();
        }


        private ParameterSetupModel CreateParameter(ParameterMetadata descriptor)
        {
            if (!descriptor.IsValid)
                return new ParameterSetupModel.Invalid(descriptor);

            if (descriptor.Descriptor.IsEnum)
                return new EnumParamSetupModel(descriptor);
            if (descriptor.Descriptor.DataType == ParameterSetupModel.NullableIntTypeName)
                return new NullableIntParamSetupModel(descriptor);
            if (descriptor.Descriptor.DataType == ParameterSetupModel.NullableDoubleTypeName)
                return new NullableDoubleParamSetupModel(descriptor);

            switch (descriptor.Descriptor.DataType)
            {
                case "System.Boolean": return new BoolParamSetupModel(descriptor);
                case "System.Int32": return new IntParamSetupModel(descriptor);
                case "System.Double": return new DoubleParamSetupModel(descriptor);
                case "System.String": return new StringParamSetupModel(descriptor);
                case "TickTrader.Algo.Api.File": return new FileParamSetupModel(descriptor);
                default: return new ParameterSetupModel.Invalid(descriptor, ErrorMsgCodes.UnsupportedParameterType);
            }
        }

        private InputSetupModel CreateBarBasedInput(InputMetadata descriptor)
        {
            if (!descriptor.IsValid)
                return new InputSetupModel.Invalid(descriptor);

            switch (descriptor.Descriptor.DataSeriesBaseTypeFullName)
            {
                case "System.Double": return new BarToDoubleInputSetupModel(descriptor, Metadata, Context.DefaultSymbolCode, $"{Context.DefaultMapping}.Close");
                case "TickTrader.Algo.Api.Bar": return new BarToBarInputSetupModel(descriptor, Metadata, Context.DefaultSymbolCode, Context.DefaultMapping);
                //case "TickTrader.Algo.Api.Quote": return new QuoteInputSetupModel(descriptor, Metadata, DefaultSymbolCode, false);
                //case "TickTrader.Algo.Api.QuoteL2": return new QuoteInputSetupModel(descriptor, Metadata, DefaultSymbolCode, true);
                default: return new InputSetupModel.Invalid(descriptor, ErrorMsgCodes.UnsupportedInputType);
            }
        }

        private InputSetupModel CreateTickBasedInput(InputMetadata descriptor)
        {
            if (!descriptor.IsValid)
                return new InputSetupModel.Invalid(descriptor);

            switch (descriptor.Descriptor.DataSeriesBaseTypeFullName)
            {
                case "System.Double": return new QuoteToDoubleInputSetupModel(descriptor, Metadata, Context.DefaultSymbolCode, $"{Context.DefaultMapping}.Close");
                case "TickTrader.Algo.Api.Bar": return new QuoteToBarInputSetupModel(descriptor, Metadata, Context.DefaultSymbolCode, Context.DefaultMapping);
                case "TickTrader.Algo.Api.Quote": return new QuoteInputSetupModel(descriptor, Metadata, Context.DefaultSymbolCode, false);
                case "TickTrader.Algo.Api.QuoteL2": return new QuoteInputSetupModel(descriptor, Metadata, Context.DefaultSymbolCode, true);
                default: return new InputSetupModel.Invalid(descriptor, ErrorMsgCodes.UnsupportedInputType);
            }
        }

        private OutputSetupModel CreateOutput(OutputMetadata descriptor)
        {
            if (!descriptor.IsValid)
                return new OutputSetupModel.Invalid(descriptor);

            switch (descriptor.Descriptor.DataSeriesBaseTypeFullName)
            {
                case "System.Double": return new ColoredLineOutputSetupModel(descriptor);
                case "TickTrader.Algo.Api.Marker": return new MarkerSeriesOutputSetupModel(descriptor);
                default: return new OutputSetupModel.Invalid(descriptor, ErrorMsgCodes.UnsupportedOutputType);
            }
        }
    }
}
