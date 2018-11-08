using System;
using System.Reflection;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.Core.Metadata
{
    [Serializable]
    public class OutputMetadata : PropertyMetadataBase
    {
        [NonSerialized]
        private Type _dataSeriesBaseType;


        public OutputDescriptor Descriptor { get; }

        public Type DataSeriesBaseType => _dataSeriesBaseType;

        public bool IsShortDefinition { get; private set; }

        public bool IsHiddenEntity { get; private set; }


        protected override PropertyDescriptor PropDescriptor => Descriptor;


        public OutputMetadata(PropertyInfo reflectionInfo, OutputAttribute attribute)
            : base(reflectionInfo)
        {
            Descriptor = new OutputDescriptor();
            InitDescriptor(reflectionInfo.Name, attribute.DisplayName);

            Validate(reflectionInfo);

            var propertyType = reflectionInfo.PropertyType;

            if (propertyType == typeof(DataSeries))
            {
                _dataSeriesBaseType = typeof(double);
                IsShortDefinition = true;
            }
            else if (propertyType == typeof(MarkerSeries))
            {
                _dataSeriesBaseType = typeof(Marker);
                IsShortDefinition = true;
            }
            else if (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(DataSeries<>))
            {
                _dataSeriesBaseType = reflectionInfo.PropertyType.GetGenericArguments()[0];
            }
            else
                SetError(AlgoPropertyErrors.OutputIsNotDataSeries);

            if (DataSeriesBaseType != null && DataSeriesBaseType.IsInterface)
                IsHiddenEntity = true;

            Descriptor.DataSeriesBaseTypeFullName = _dataSeriesBaseType?.FullName ?? string.Empty;
            Descriptor.DefaultThickness = attribute.DefaultThickness;
            Descriptor.DefaultColor = attribute.DefaultColor;
            Descriptor.DefaultLineStyle = attribute.DefaultLineStyle;
            Descriptor.PlotType = attribute.PlotType;
            Descriptor.Target = attribute.Target;
            Descriptor.Precision = attribute.Precision;
            Descriptor.ZeroLine = attribute.ZeroLine;
        }


        internal DataSeriesImpl<T> CreateOutput2<T>()
        {
            if (typeof(T) == typeof(double) && IsShortDefinition)
                return (DataSeriesImpl<T>)(object)new DataSeriesProxy();
            else if (typeof(T) == typeof(Marker) && IsShortDefinition)
                return (DataSeriesImpl<T>)(object)new MarkerSeriesProxy();
            else
                return new DataSeriesImpl<T>();
        }
    }
}
