using System;
using System.Reflection;
using TickTrader.Algo.Api;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.CoreV1.Metadata
{
    public class InputMetadata : PropertyMetadataBase
    {
        private Type _dataSeriesBaseType;


        public InputDescriptor Descriptor { get; }

        public Type DataSeriesBaseType => _dataSeriesBaseType;

        public bool IsShortDefinition { get; private set; }


        public override IPropertyDescriptor PropDescriptor => Descriptor;


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
                SetError(Domain.Metadata.Types.PropertyErrorCode.InputIsNotDataSeries);

            Descriptor.DataSeriesBaseTypeFullName = _dataSeriesBaseType?.FullName ?? string.Empty;
        }
    }
}
