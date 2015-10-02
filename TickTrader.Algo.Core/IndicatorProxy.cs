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

        public IndicatorProxy(string descriptorId, IEnumerable<AlgoProxyParam> parameters)
            : this(AlgoDescriptor.Get(descriptorId), parameters)
        {
        }

        internal IndicatorProxy(AlgoDescriptor descriptor, IEnumerable<AlgoProxyParam> parameters)
        {
            if (descriptor.AlgoLogicType != AlgoTypes.Indicator)
                throw new Exception("This is not an indicator.");

            instance = (Api.Indicator)descriptor.CreateInstance();

            var paramsById = parameters.ToDictionary(p => p.Id);

            foreach (var property in descriptor.AllProperties)
            {
                AlgoProxyParam param;
                if (!paramsById.TryGetValue(property.Id, out param))
                {
                    if (property.PropertyType == AlgoPropertyTypes.InputSeries
                        || property.PropertyType == AlgoPropertyTypes.OutputSeries)
                        throw new Exception("Property " + property.Id + " is not initialized.");

                    // TO DO : set default value
                }

                property.Set(instance, param.Value);
            }
        }

        public void InvokeCalculate()
        {
            instance.DoCalculate();
        }
    }

    [Serializable]
    public class AlgoProxyParam
    {
        public string Id { get; set; }
        public object Value { get; set; }
    }
}
