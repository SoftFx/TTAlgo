using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TickTrader.Algo.Api;
using TickTrader.Algo.Common.Model.Config;
using TickTrader.Algo.Common.Model.Library;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Metadata;
using TickTrader.Algo.Core.Repository;

namespace TickTrader.Algo.Common.Model.Setup
{
    public abstract class PluginSetupModel
    {
        private List<PropertySetupModel> _allProperties;
        private List<ParameterSetupModel> _parameters;
        private List<InputSetupModel> _barBasedInputs;
        private List<InputSetupModel> _tickBasedInputs;
        private List<OutputSetupModel> _outputs;


        public TimeFrames SelectedTimeFrame { get; protected set; }

        public string MainSymbol { get; protected set; }

        public Mapping SelectedMapping { get; protected set; }

        public IEnumerable<ParameterSetupModel> Parameters => _parameters;

        public IEnumerable<InputSetupModel> Inputs => ActiveInputs;

        public IEnumerable<OutputSetupModel> Outputs => _outputs;

        public PluginMetadata Metadata { get; }

        public AlgoPluginRef PluginRef { get; }

        public bool IsValid { get; protected set; }

        public IAlgoSetupMetadata SetupMetadata { get; }

        public IAlgoSetupContext SetupContext { get; }

        public string InstanceId { get; protected set; }

        public PluginPermissions Permissions { get; protected set; }

        public bool AllowTrade
        {
            get { return Permissions.TradeAllowed; }
            set { Permissions.TradeAllowed = value; }
        }

        public bool Isolated
        {
            get { return Permissions.Isolated; }
            set { Permissions.Isolated = value; }
        }


        protected List<InputSetupModel> ActiveInputs => SelectedTimeFrame == TimeFrames.Ticks ? _tickBasedInputs : _barBasedInputs;


        public event Action ValidityChanged = delegate { };


        public PluginSetupModel(AlgoPluginRef pRef, IAlgoSetupMetadata metadata, IAlgoSetupContext context)
        {
            PluginRef = pRef;
            Metadata = pRef.Metadata;
            SetupMetadata = metadata;
            SetupContext = context;
        }


        public virtual void Apply(IPluginSetupTarget target)
        {
            _parameters.ForEach(p => p.Apply(target));
            _outputs.ForEach(p => p.Apply(target));
            ActiveInputs.Foreach(p => p.Apply(target));
        }

        public virtual void Load(PluginConfig cfg)
        {
            SelectedTimeFrame = cfg.TimeFrame;
            MainSymbol = cfg.MainSymbol;
            SelectedMapping = SetupMetadata.Mappings.GetBarToBarMappingOrDefault(cfg.SelectedMapping);
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
            cfg.MainSymbol = MainSymbol;
            cfg.SelectedMapping = SelectedMapping.Key;
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
            SelectedTimeFrame = SetupContext.DefaultTimeFrame;
            MainSymbol = SetupContext.DefaultSymbolCode;
            SelectedMapping = SetupMetadata.Mappings.GetBarToBarMappingOrDefault(SetupContext.DefaultMapping);
            InstanceId = SetupMetadata.IdProvider.GeneratePluginId(Metadata.Descriptor);

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
            return Metadata.Descriptor.IsValid && _allProperties.All(p => !p.HasError);
        }


        protected void Init()
        {
            _parameters = Metadata.Parameters.Select(CreateParameter).ToList();
            _barBasedInputs = Metadata.Inputs.Select(CreateBarBasedInput).ToList();
            _tickBasedInputs = Metadata.Inputs.Select(CreateTickBasedInput).ToList();
            _outputs = Metadata.Outputs.Select(CreateOutput).ToList();

            _allProperties = _parameters.Concat<PropertySetupModel>(_barBasedInputs).Concat(_tickBasedInputs).Concat(_outputs).ToList();
            _allProperties.ForEach(p => p.ErrorChanged += s => Validate());

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
                case "System.Double": return new BarToDoubleInputSetupModel(descriptor, SetupMetadata, SetupContext);
                case "TickTrader.Algo.Api.Bar": return new BarToBarInputSetupModel(descriptor, SetupMetadata, SetupContext);
                //case "TickTrader.Algo.Api.Quote": return new QuoteInputSetupModel(descriptor, Metadata, SetupContext, false);
                //case "TickTrader.Algo.Api.QuoteL2": return new QuoteInputSetupModel(descriptor, Metadata, SetupContext, true);
                default: return new InputSetupModel.Invalid(descriptor, ErrorMsgCodes.UnsupportedInputType);
            }
        }

        private InputSetupModel CreateTickBasedInput(InputMetadata descriptor)
        {
            if (!descriptor.IsValid)
                return new InputSetupModel.Invalid(descriptor);

            switch (descriptor.Descriptor.DataSeriesBaseTypeFullName)
            {
                case "System.Double": return new QuoteToDoubleInputSetupModel(descriptor, SetupMetadata, SetupContext);
                case "TickTrader.Algo.Api.Bar": return new QuoteToBarInputSetupModel(descriptor, SetupMetadata, SetupContext);
                case "TickTrader.Algo.Api.Quote": return new QuoteInputSetupModel(descriptor, SetupMetadata, SetupContext, false);
                case "TickTrader.Algo.Api.QuoteL2": return new QuoteInputSetupModel(descriptor, SetupMetadata, SetupContext, true);
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
