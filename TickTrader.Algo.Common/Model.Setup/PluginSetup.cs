using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Common.Lib;
using TickTrader.Algo.Common.Model.Config;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Metadata;
using TickTrader.Algo.Core.Repository;
using Api = TickTrader.Algo.Api;

namespace TickTrader.Algo.Common.Model.Setup
{
    [DataContract(Name = "algoSetup", Namespace = "")]
    [KnownType(typeof(BarBasedPluginSetup))]
    public abstract class PluginSetup : ObservableObject
    {
        [DataMember(Name = "properties")]
        private List<PropertySetupBase> allProperties;
        private List<ParameterSetup> parameters;
        private List<InputSetup> inputs;
        private List<OutputSetup> outputs;

        public PluginSetup(AlgoPluginRef pRef)
        {
            this.PluginRef = pRef;
            this.Descriptor = pRef.Descriptor;
        }

        protected void Init()
        {
            parameters = Descriptor.Parameters.Select(ParameterSetup.Create).ToList();
            inputs = Descriptor.Inputs.Select(CreateInput).ToList();
            outputs = Descriptor.Outputs.Select(CreateOuput).ToList();

            allProperties = parameters.Concat<PropertySetupBase>(inputs).Concat(outputs).ToList();
            allProperties.ForEach(p => p.ErrorChanged += s => Validate());

            IsEmpty = allProperties.Count == 0;

            Reset();
            Validate();
        }

        protected abstract InputSetup CreateInput(InputDescriptor descriptor);
        protected abstract OutputSetup CreateOuput(OutputDescriptor descriptor);

        public void Reset()
        {
            foreach (var p in allProperties)
                p.Reset();
        }

        private void Validate()
        {
            IsValid = Descriptor.Error == null && Parameters.All(p => !p.HasError);
            ValidityChanged();
        }

        public virtual void Apply(IPluginSetupTarget target)
        {
            allProperties.ForEach(p => p.Apply(target));
        }

        public virtual void Load(PluginConfig cfg)
        {
            foreach (var scrProperty in cfg.Properties)
            {
                var thisProperty = this.allProperties.FirstOrDefault(p => p.Id == scrProperty.Id);
                if (thisProperty != null)
                    thisProperty.Load(scrProperty);
            }
        }

        public virtual PluginConfig Save()
        {
            var cfg = SaveToConfig();
            foreach (var property in allProperties)
                cfg.Properties.Add(property.Save());
            return cfg;
        }

        protected abstract PluginConfig SaveToConfig();

        public IEnumerable<ParameterSetup> Parameters { get { return parameters; } }
        public IEnumerable<InputSetup> Inputs { get { return inputs; } }
        public IEnumerable<PropertySetupBase> Outputs { get { return outputs; } }
        public bool HasInputsOrParams { get { return HasParams || HasInputs; } }
        public bool HasParams { get { return parameters.Count > 0; } }
        public bool HasInputs { get { return inputs.Count > 0; } }
        public bool HasOutputs { get { return outputs.Count > 0; } }
        public AlgoPluginDescriptor Descriptor { get; private set; }
        public AlgoPluginRef PluginRef { get; private set; }

        public bool IsValid { get; private set; }
        public bool IsEmpty { get; private set; }
        public event Action ValidityChanged = delegate { };
    }
}
