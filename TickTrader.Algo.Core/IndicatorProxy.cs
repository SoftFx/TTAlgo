using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Core.Metadata;

namespace TickTrader.Algo.Core
{
    public abstract class AlgoProxy : NoTimeoutByRefObject
    {
        private IAlgoContext context;

        public AlgoProxy(AlgoDescriptor descriptor, IAlgoContext context)
        {
            this.context = context;
            Descriptor = descriptor;
        }

        internal abstract Algo.Api.Algo Instance { get; }
        internal AlgoDescriptor Descriptor { get; private set; }

        internal void BindUpParameters(Api.Algo instance)
        {
            foreach (var paramProperty in Descriptor.Parameters)
            {
                paramProperty.Set(instance, context.GetParameter(paramProperty.Id));
            }
        }

        internal void BindUpInputs(Api.Algo instance)
        {
            foreach (var inputProperty in Descriptor.Inputs)
                ReflectGenericMethod(inputProperty.DatdaSeriesBaseType, "BindInput", inputProperty);
        }

        internal void BindUpOutputs(Api.Algo instance)
        {
            foreach (var outputProperty in Descriptor.Outputs)
                ReflectGenericMethod(outputProperty.DatdaSeriesBaseType, "BindOutput", outputProperty);
        }

        public void BindInput<T>(InputDescriptor d)
        {
            var buffer = d.CreateInput<T>();
            d.Set(Instance, buffer);
            context.BindInput<T>(d.Id, buffer);
        }

        public void BindOutput<T>(OutputDescriptor d)
        {
            var buffer = d.CreateOutput<T>();
            d.Set(Instance, buffer);
            context.BindOutput<T>(d.Id, buffer);
        }

        private void ReflectGenericMethod(Type genericType, string methodName, params object[] parameters)
        {
            MethodInfo method = typeof(AlgoProxy).GetMethod(methodName);
            MethodInfo genericMethod = method.MakeGenericMethod(genericType);
            genericMethod.Invoke(this, parameters);
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

        internal override Api.Algo Instance { get { return instance; } }

        public void InvokeCalculate()
        {
            instance.DoCalculate();
        }
    }
}
