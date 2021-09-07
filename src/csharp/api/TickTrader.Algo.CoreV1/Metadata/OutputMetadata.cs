using System;
using System.Reflection;
using TickTrader.Algo.Api;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.CoreV1.Metadata
{
    public class OutputMetadata : PropertyMetadataBase
    {
        private Type _dataSeriesBaseType;


        public OutputDescriptor Descriptor { get; }

        public Type DataSeriesBaseType => _dataSeriesBaseType;

        public bool IsShortDefinition { get; private set; }

        public bool IsHiddenEntity { get; private set; }


        public override IPropertyDescriptor PropDescriptor => Descriptor;


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
                SetError(Domain.Metadata.Types.PropertyErrorCode.OutputIsNotDataSeries);

            if (DataSeriesBaseType != null && DataSeriesBaseType.IsInterface)
                IsHiddenEntity = true;

            Descriptor.DataSeriesBaseTypeFullName = _dataSeriesBaseType?.FullName ?? string.Empty;
            Descriptor.DefaultThickness = attribute.DefaultThickness;
            Descriptor.DefaultColorArgb = attribute.DefaultColor.ToArgb();
            Descriptor.DefaultLineStyle = attribute.DefaultLineStyle.ToDomainEnum();
            Descriptor.PlotType = attribute.PlotType.ToDomainEnum();
            Descriptor.Target = attribute.Target.ToDomainEnum();
            Descriptor.Precision = attribute.Precision;
            Descriptor.ZeroLine = attribute.ZeroLine;
            Descriptor.Visibility = attribute.Visibility;
        }
    }
}
