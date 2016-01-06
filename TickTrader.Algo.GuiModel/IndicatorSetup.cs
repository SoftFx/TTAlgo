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
    public class IndicatorSetup : ObservableObject
    {
        private List<ParameterSetup> parameters;
        private List<InputSetup> inputs;

        public IndicatorSetup(AlgoInfo descriptor, Func<InputInfo, InputSetup> inputSetupFactory, IndicatorSetup copyFrom = null)
        {
            this.Descriptor = descriptor;

            parameters = descriptor.Parameters.Select(ParameterSetup.Create).ToList();
            parameters.ForEach(p => p.ErrorChanged += s => Validate());

            inputs = descriptor.Inputs.Select(inputSetupFactory).ToList();
            inputs.ForEach(i => i.ErrorChanged += s => Validate());

            Validate();

            if (copyFrom != null)
                CopyParametersFrom(copyFrom);
        }

        //public Func<InputInfo, InputSetup> InputSetupFactory { get; set; }

        public void Reset()
        {
            foreach (var p in Parameters)
                p.Reset();
        }

        public void CopyParametersFrom(IndicatorSetup otherSetup)
        {
            foreach (var otherParam in otherSetup.Parameters)
            {
                var param = this.Parameters.FirstOrDefault(p => p.Id == otherParam.Id);
                if (param != null && param.DataType == otherParam.DataType)
                    param.ValueObj = otherParam.ValueObj;
            }
        }

        private void Validate()
        {
            IsValid = Descriptor.Error == null && Parameters.All(p => !p.HasError);
            ValidityChanged();
        }

        //public IList<AlgoInputSetup> Inputs { get; private set; }
        //public IList<AlgoOutputSetup> Outputs { get; private set; }
        public IEnumerable<ParameterSetup> Parameters { get { return parameters; } }
        public IEnumerable<InputSetup> Inputs { get { return inputs; } }
        public AlgoInfo Descriptor { get; private set; }

        public bool IsValid {get; private set;}
        public event Action ValidityChanged = delegate { };
    }

}
