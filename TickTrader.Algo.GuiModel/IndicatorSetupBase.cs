using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Metadata;
using TickTrader.Algo.Core.Repository;
using Api = TickTrader.Algo.Api;

namespace TickTrader.Algo.GuiModel
{
    public abstract class IndicatorSetupBase : ObservableObject
    {
        private List<PropertySetupBase> allProperties;
        private List<ParameterSetup> parameters;
        private List<InputSetup> inputs;
        private List<OutputSetup> outputs;

        public IndicatorSetupBase(AlgoInfo descriptor)
        {
            this.Descriptor = descriptor;

            parameters = descriptor.Parameters.Select(ParameterSetup.Create).ToList();
            inputs = descriptor.Inputs.Select(CreateInput).ToList();
            outputs = descriptor.Outputs.Select(CreateOuput).ToList();

            allProperties = parameters.Concat<PropertySetupBase>(inputs).Concat(outputs).ToList();
            allProperties.ForEach(p => p.ErrorChanged += s => Validate());

            Validate();
        }

        protected abstract InputSetup CreateInput(InputInfo descriptor);
        protected abstract OutputSetup CreateOuput(OutputInfo descriptor);

        public void Reset()
        {
            foreach (var p in Parameters)
                p.Reset();
        }

        public virtual void CopyFrom(IndicatorSetupBase srcSetup)
        {
            foreach (var scrProperty in srcSetup.allProperties)
            {
                var thisProperty = this.allProperties.FirstOrDefault(p => p.Id == scrProperty.Id);
                if (thisProperty != null)
                    thisProperty.CopyFrom(scrProperty);
            }
        }

        private void Validate()
        {
            IsValid = Descriptor.Error == null && Parameters.All(p => !p.HasError);
            ValidityChanged();
        }

        public IEnumerable<ParameterSetup> Parameters { get { return parameters; } }
        public IEnumerable<InputSetup> Inputs { get { return inputs; } }
        public IEnumerable<PropertySetupBase> Outputs { get { return outputs; } }
        public AlgoInfo Descriptor { get; private set; }

        public bool IsValid { get; private set; }
        public event Action ValidityChanged = delegate { };
    }
}
