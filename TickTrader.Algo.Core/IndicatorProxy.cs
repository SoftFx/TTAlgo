using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Core.Metadata;

namespace TickTrader.Algo.Core
{
    public class AlgoProxy : NoTimeoutByRefObject
    {
        private IAlgoContext context;

        public AlgoProxy(string algoId, IAlgoContext context)
        {
            this.context = context;
            Descriptor = AlgoDescriptor.Get(algoId);

            context.Init();
        }

        internal AlgoDescriptor Descriptor { get; private set; }

        public IAlgoContext Context { get { return context; } }
        //public IAlgoDataReader Reader { get { return setup.Reader; } }
        //public IAlgoDataWriter Writer { get { return setup.Writer; } }

        internal void BindUpParameters(Api.Algo instance)
        {
            foreach (var paramProperty in Descriptor.Parameters)
                paramProperty.Set(instance, context.GetParameter(paramProperty.Id));
        }

        internal void BindUpInputs(Api.Algo instance)
        {
            foreach (var inputProperty in Descriptor.Inputs)
                inputProperty.Set(instance, context.BindInput(inputProperty.Id, inputProperty));
        }

        internal void BindUpOutputs(Api.Algo instance)
        {
            foreach (var outputProperty in Descriptor.Outputs)
                outputProperty.Set(instance, context.BindOutput(outputProperty.Id, outputProperty));
        }
    }

    public class IndicatorProxy : AlgoProxy
    {
        private Api.Indicator instance;

        public IndicatorProxy(string algoId, IAlgoContext context)
            : base(algoId, context)
        {
            if (Descriptor.AlgoLogicType != AlgoTypes.Indicator)
                throw new Exception("This is not an indicator.");

            instance = (Api.Indicator)Descriptor.CreateInstance();

            BindUpParameters(instance);
            BindUpInputs(instance);
            BindUpOutputs(instance);
        }

        public void InvokeCalculate()
        {
            instance.DoCalculate();
        }
    }
}
