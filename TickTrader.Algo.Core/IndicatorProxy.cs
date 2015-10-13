using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;
using TickTrader.Algo.Core.Metadata;

namespace TickTrader.Algo.Core
{
    public class IndicatorProxy
    {
        private Api.Indicator instance;

        public IndicatorProxy(string descriptorId, IAlgoContext context)
            : this(AlgoDescriptor.Get(descriptorId), context)
        {
        }

        internal IndicatorProxy(AlgoDescriptor descriptor, IAlgoContext context)
        {
            if (descriptor.AlgoLogicType != AlgoTypes.Indicator)
                throw new Exception("This is not an indicator.");

            // create instance

            instance = (Api.Indicator)descriptor.CreateInstance();

            // init parameters

            foreach (var paramProperty in descriptor.Parameters)
            {
                object paramVal;
                if (!context.GetParameter(paramProperty.Id, out paramVal))
                    paramVal = paramProperty.DefaultValue;
                paramProperty.Set(instance, paramVal);
            }
            
            // init inputs

            foreach (var inputProperty in descriptor.Inputs)
                inputProperty.Set(instance, context.GetInputSeries(inputProperty.Id));

            // init outputs

            foreach (var inputProperty in descriptor.Inputs)
                inputProperty.Set(instance, context.GetInputSeries(inputProperty.Id));
        }

        public void InvokeCalculate()
        {
            instance.DoCalculate();
        }
    }

    public interface IAlgoContext
    {
        bool GetParameter(string paramId, out object paramValue);
        object GetInputSeries(string inputId);
        object GetOutputSeries(string inputId);
    }
}
