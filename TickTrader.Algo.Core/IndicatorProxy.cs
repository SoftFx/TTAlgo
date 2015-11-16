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
        private static readonly DataSeriesFactory factoryInstance = new DataSeriesFactory();

        private AlgoContext setup;

        public AlgoProxy(string algoId, AlgoContext setup)
        {
            this.setup = setup;
            Descriptor = AlgoDescriptor.Get(algoId);
        }

        internal AlgoDescriptor Descriptor { get; private set; }
        public IAlgoDataReader Reader { get { return setup.Reader; } }
        public IAlgoDataWriter Writer { get { return setup.Writer; } }

        internal void BindUpParameters(Api.Algo instance)
        {
            foreach (var paramProperty in Descriptor.Parameters)
            {
                object paramVal;
                if (!setup.Parameters.TryGetValue(paramProperty.Id, out paramVal))
                    paramVal = paramProperty.DefaultValue;
                paramProperty.Set(instance, paramVal);
            }
        }

        internal void BindUpInputs(Api.Algo instance)
        {
            if (Reader == null)
                throw new InvalidOperationException("Data Reader is not set!");

            foreach (var inputProperty in Descriptor.Inputs)
                inputProperty.Set(instance, Reader.GetInputBuffer(inputProperty.Id));
        }

        internal void BindUpOutputs(Api.Algo instance)
        {
            if (Writer == null)
                throw new InvalidOperationException("Data Writer is not set!");

            foreach (var outputProperty in Descriptor.Outputs)
                outputProperty.Set(instance, Writer.GetOutputBuffer(outputProperty.Id));
        }
    }

    public class IndicatorProxy : AlgoProxy
    {
        private Api.Indicator instance;

        public IndicatorProxy(string algoId, AlgoContext setup)
            : base(algoId, setup)
        {
            if (Descriptor.AlgoLogicType != AlgoTypes.Indicator)
                throw new Exception("This is not an indicator.");

            instance = (Api.Indicator)Descriptor.CreateInstance();

            BindUpParameters(instance);
            BindUpInputs(instance);
            BindUpParameters(instance);
        }

        public void InvokeCalculate()
        {
            instance.DoCalculate();
        }
    }
}
