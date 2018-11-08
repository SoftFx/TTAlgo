using System;
using System.Reflection;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.Core.Metadata
{
    [Serializable]
    public class InputMetadata : PropertyMetadataBase
    {
        [NonSerialized]
        private Type _dataSeriesBaseType;


        public InputDescriptor Descriptor { get; }

        public Type DataSeriesBaseType => _dataSeriesBaseType;

        public bool IsShortDefinition { get; private set; }


        protected override PropertyDescriptor PropDescriptor => Descriptor;


        public InputMetadata(PropertyInfo reflectionInfo, InputAttribute attribute)
            : base(reflectionInfo)
        {
            Descriptor = new InputDescriptor();
            InitDescriptor(reflectionInfo.Name, attribute.DisplayName);

            Validate(reflectionInfo);

            var propertyType = reflectionInfo.PropertyType;

            if (propertyType == typeof(DataSeries))
            {
                _dataSeriesBaseType = typeof(double);
                IsShortDefinition = true;
            }
            else if (propertyType == typeof(TimeSeries))
            {
                _dataSeriesBaseType = typeof(DateTime);
                IsShortDefinition = true;
            }
            else if (propertyType == typeof(BarSeries))
            {
                _dataSeriesBaseType = typeof(Bar);
                IsShortDefinition = true;
            }
            else if (propertyType == typeof(QuoteSeries))
            {
                _dataSeriesBaseType = typeof(Quote);
                IsShortDefinition = true;
            }
            else if (propertyType == typeof(QuoteL2Series))
            {
                _dataSeriesBaseType = typeof(Quote);
                IsShortDefinition = true;
            }
            else if (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(DataSeries<>))
                _dataSeriesBaseType = reflectionInfo.PropertyType.GetGenericArguments()[0];
            else
                SetError(AlgoPropertyErrors.InputIsNotDataSeries);

            Descriptor.DataSeriesBaseTypeFullName = _dataSeriesBaseType?.FullName ?? string.Empty;
        }


        internal DataSeriesImpl<T> CreateInput2<T>()
        {
            if (typeof(T) == typeof(double) && IsShortDefinition)
                return (DataSeriesImpl<T>)(object)new DataSeriesProxy();
            else if (typeof(T) == typeof(DateTime) && IsShortDefinition)
                return (DataSeriesImpl<T>)(object)new TimeSeriesProxy();
            else if (typeof(T) == typeof(Bar) && IsShortDefinition)
                return (DataSeriesImpl<T>)(object)new BarSeriesProxy();
            else if (typeof(T) == typeof(Quote) && IsShortDefinition)
                return (DataSeriesImpl<T>)(object)new QuoteSeriesProxy();
            else
                return new DataSeriesImpl<T>();
        }
    }
}
