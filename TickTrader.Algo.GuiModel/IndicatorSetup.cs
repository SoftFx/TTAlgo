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
    public abstract class IndicatorSetup : ObservableObject
    {
        private List<ParameterSetup> parameters;

        public IndicatorSetup(AlgoInfo descriptor)
        {
            this.Descriptor = descriptor;

            parameters = descriptor.Parameters.Select(ParameterSetup.Create).ToList();
            parameters.ForEach(p => p.ErrorChanged += s => Validate());

            foreach (var i in descriptor.Inputs)
                CreateInput(i);

            foreach (var i in Inputs)
                i.ErrorChanged += s => Validate();

            Validate();
        }

        protected abstract void CreateInput(InputInfo descriptor);

        public abstract IEnumerable<InputSetup> Inputs { get; }

        public void Reset()
        {
            foreach (var p in Parameters)
                p.Reset();
        }

        public virtual void CopyFrom(IndicatorSetup otherSetup)
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

        public IEnumerable<ParameterSetup> Parameters { get { return parameters; } }
        public AlgoInfo Descriptor { get; private set; }

        public bool IsValid { get; private set; }
        public event Action ValidityChanged = delegate { };
    }

    public abstract class BarIndicatorSetup : IndicatorSetup
    {
        private List<BarInputSetup> inputs = new List<BarInputSetup>();

        public BarIndicatorSetup(AlgoInfo descriptor)
            : base(descriptor)
        {
        }

        public override IEnumerable<InputSetup> Inputs { get { return inputs; } }

        protected abstract BarInputSetup CreateBarInput(InputInfo descriptor);

        protected override sealed void CreateInput(InputInfo descriptor)
        {
            BarInputSetup inputSetup;

            if (!descriptor.IsValid)
                inputSetup = new BarInputSetup.Invalid(descriptor, new LocMsg(descriptor.Error.Value));
            else
                inputSetup = CreateBarInput(descriptor);

            inputs.Add(inputSetup);
        }

        public void ConfigureReader(DirectReader<Api.Bar> reader)
        {
            inputs.ForEach(r => r.Configure(reader));
        }
    }
}
