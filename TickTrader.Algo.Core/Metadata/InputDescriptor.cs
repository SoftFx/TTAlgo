using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;
using TickTrader.Algo.Core.DataflowConcept;

namespace TickTrader.Algo.Core.Metadata
{
    public class InputDescriptor : AlgoPropertyDescriptor
    {
        public InputDescriptor(AlgoPluginDescriptor classMetadata, PropertyInfo propertyInfo, object attribute)
            : base(classMetadata, propertyInfo)
        {
            Attribute = (InputAttribute)attribute;
            Validate(propertyInfo);

            var propertyType = propertyInfo.PropertyType;

            if (propertyType == typeof(DataSeries))
            {
                DatdaSeriesBaseType = typeof(double);
                IsShortDefinition = true;
            }
            else if (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(DataSeries<>))
                DatdaSeriesBaseType = propertyInfo.PropertyType.GetGenericArguments()[0];
            else
                SetError(Metadata.AlgoPropertyErrors.InputIsNotDataSeries);

            InitDisplayName(Attribute.DisplayName);
        }

        public Type DatdaSeriesBaseType { get; private set; }
        public string DataSeriesBaseTypeFullName { get { return DatdaSeriesBaseType.FullName; } }
        public bool IsShortDefinition { get; private set; }
        public InputAttribute Attribute { get; private set; }
        public override AlgoPropertyTypes PropertyType { get { return AlgoPropertyTypes.InputSeries; } }

        internal DataSeriesBuffer<T> CreateInput<T>()
        {
            if (typeof(T) == typeof(double) && IsShortDefinition)
                return (InputDataSeries<T>)(object)new InputDataSeries();
            else
                return new InputDataSeries<T>();
        }

        internal DataSeriesProxy<T> CreateInput2<T>()
        {
            if (typeof(T) == typeof(double) && IsShortDefinition)
                return (DataSeriesProxy<T>)(object)new DataSeriesProxy();
            else
                return new DataSeriesProxy<T>();
        }
    }
}
