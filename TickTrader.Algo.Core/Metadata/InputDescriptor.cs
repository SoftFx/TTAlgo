using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.Core.Metadata
{
    [Serializable]
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
            else if (propertyType == typeof(TimeSeries))
            {
                DatdaSeriesBaseType = typeof(DateTime);
                IsShortDefinition = true;
            }
            else if (propertyType == typeof(BarSeries))
            {
                DatdaSeriesBaseType = typeof(Api.Bar);
                IsShortDefinition = true;
            }
            else if (propertyType == typeof(QuoteSeries))
            {
                DatdaSeriesBaseType = typeof(Api.Quote);
                IsShortDefinition = true;
            }
            else if (propertyType == typeof(QuoteL2Series))
            {
                DatdaSeriesBaseType = typeof(Api.Quote);
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

        internal DataSeriesProxy<T> CreateInput2<T>()
        {
            if (typeof(T) == typeof(double) && IsShortDefinition)
                return (DataSeriesProxy<T>)(object)new DataSeriesProxy();
            else if (typeof(T) == typeof(DateTime) && IsShortDefinition)
                return (DataSeriesProxy<T>)(object)new TimeSeriesProxy();
            else if (typeof(T) == typeof(Api.Bar) && IsShortDefinition)
                return (DataSeriesProxy<T>)(object)new BarSeriesProxy();
            else if (typeof(T) == typeof(Api.Quote) && IsShortDefinition)
                return (DataSeriesProxy<T>)(object)new QuoteSeriesProxy();
            else
                return new DataSeriesProxy<T>();
        }
    }
}
