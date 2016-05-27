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
    public class OutputDescriptor : AlgoPropertyDescriptor
    {
        public OutputDescriptor(AlgoPluginDescriptor classMetadata, PropertyInfo propertyInfo, OutputAttribute attribute)
            : base(classMetadata, propertyInfo)
        {
            Validate(propertyInfo);

            var propertyType = propertyInfo.PropertyType;

            if (propertyType == typeof(DataSeries))
            {
                DataSeriesBaseType = typeof(double);
                IsShortDefinition = true;
            }
            else if (propertyType == typeof(MarkerSeries))
            {
                DataSeriesBaseType = typeof(Marker);
                IsShortDefinition = true;
            }
            else if (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(DataSeries<>))
            {
                DataSeriesBaseType = propertyInfo.PropertyType.GetGenericArguments()[0];
            }
            else
                SetError(Metadata.AlgoPropertyErrors.OutputIsNotDataSeries);

            if (DataSeriesBaseType != null && DataSeriesBaseType.IsInterface)
                IsHiddenEntity = true;

            this.DefaultThickness = attribute.DefaultThickness;
            this.DefaultColor = attribute.DefaultColor;
            this.DefaultLineStyle = attribute.DefaultLineStyle;
            this.PlotType = attribute.PlotType;

            InitDisplayName(attribute.DisplayName);
        }

        public Type DataSeriesBaseType { get; private set; }
        public string DataSeriesBaseTypeFullName { get { return DataSeriesBaseType.FullName; } }
        public bool IsShortDefinition { get; private set; }
        public bool IsHiddenEntity { get; private set; }
        public double DefaultThickness { get; private set; }
        public Colors DefaultColor { get; private set; }
        public LineStyles DefaultLineStyle { get; private set; }
        public PlotType PlotType { get; private set; }
        public override AlgoPropertyTypes PropertyType { get { return AlgoPropertyTypes.OutputSeries; } }

        internal DataSeriesProxy<T> CreateOutput2<T>()
        {
            if (typeof(T) == typeof(double) && IsShortDefinition)
                return (DataSeriesProxy<T>)(object)new DataSeriesProxy();
            else if (typeof(T) == typeof(Marker) && IsShortDefinition)
                return (DataSeriesProxy<T>)(object)new MarkerSeriesProxy();
            else
                return new DataSeriesProxy<T>();
        }
    }
}
