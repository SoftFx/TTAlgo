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

        public AlgoProxy(AlgoDescriptor descriptor, IAlgoContext context)
        {
            this.context = context;
            Descriptor = descriptor;

            context.Init();
        }

        internal AlgoDescriptor Descriptor { get; private set; }

        public IAlgoContext Context { get { return context; } }

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
            : this(AlgoDescriptor.Get(algoId), context)
        {
        }

        public IndicatorProxy(Type indicatorType, IAlgoContext context)
            : this(AlgoDescriptor.Get(indicatorType), context)
        {
        }

        public IndicatorProxy(AlgoDescriptor descriptor, IAlgoContext context)
            : base(descriptor, context)
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
