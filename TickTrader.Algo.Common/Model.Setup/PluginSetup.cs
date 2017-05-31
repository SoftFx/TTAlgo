using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using TickTrader.Algo.Common.Lib;
using TickTrader.Algo.Common.Model.Config;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Metadata;

namespace TickTrader.Algo.Common.Model.Setup
{
    [DataContract(Name = "algoSetup", Namespace = "")]
    [KnownType(typeof(BarBasedPluginSetup))]
    public abstract class PluginSetup : ObservableObject
    {
        [DataMember(Name = "properties")]
        private List<PropertySetupBase> _allProperties;
        private List<ParameterSetup> _parameters;
        private List<InputSetup> _inputs;
        private List<OutputSetup> _outputs;

        public PluginSetup(AlgoPluginRef pRef)
        {
            PluginRef = pRef;
            Descriptor = pRef.Descriptor;
        }

        protected void Init()
        {
            _parameters = Descriptor.Parameters.Select(ParameterSetup.Create).ToList();
            _inputs = Descriptor.Inputs.Select(CreateInput).ToList();
            _outputs = Descriptor.Outputs.Select(CreateOuput).ToList();

            _allProperties = _parameters.Concat<PropertySetupBase>(_inputs).Concat(_outputs).ToList();
            _allProperties.ForEach(p => p.ErrorChanged += s => Validate());

            IsEmpty = _allProperties.Count == 0;

            Reset();
            Validate();
        }

        protected abstract InputSetup CreateInput(InputDescriptor descriptor);
        protected abstract OutputSetup CreateOuput(OutputDescriptor descriptor);

        public void Reset()
        {
            foreach (var p in _allProperties)
                p.Reset();
        }

        private void Validate()
        {
            IsValid = Descriptor.Error == null && Parameters.All(p => !p.HasError);
            ValidityChanged();
        }

        public virtual void Apply(IPluginSetupTarget target)
        {
            _allProperties.ForEach(p => p.Apply(target));
        }

        public virtual void Load(PluginConfig cfg)
        {
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
            foreach (var property in _allProperties)
                cfg.Properties.Add(property.Save());
            return cfg;
        }

        public void SetWorkingFolder(string workingFolder)
        {
            foreach (FileParamSetup fileParam in _parameters.Where(p => p is FileParamSetup))
            {
                if (Path.GetFullPath(fileParam.FilePath) != fileParam.FilePath)
                {
                    fileParam.FilePath = Path.Combine(workingFolder, fileParam.FileName);
                }
            }
        }

        protected abstract PluginConfig SaveToConfig();

        public IEnumerable<ParameterSetup> Parameters => _parameters;
        public IEnumerable<InputSetup> Inputs => _inputs;
        public IEnumerable<OutputSetup> Outputs => _outputs;
        public bool HasInputsOrParams => HasParams || HasInputs;
        public bool HasParams => _parameters.Count > 0;
        public bool HasInputs => _inputs.Count > 0;
        public bool HasOutputs => _outputs.Count > 0;
        public bool HasDescription => !string.IsNullOrWhiteSpace(Descriptor?.Description);
        public AlgoPluginDescriptor Descriptor { get; private set; }
        public AlgoPluginRef PluginRef { get; private set; }

        public bool IsValid { get; private set; }
        public bool IsEmpty { get; private set; }
        public event Action ValidityChanged = delegate { };
    }
}
