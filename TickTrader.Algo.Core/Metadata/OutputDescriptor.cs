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
                DatdaSeriesBaseType = typeof(double);
                IsShortDefinition = true;
            }
            else if (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(DataSeries<>))
                DatdaSeriesBaseType = propertyInfo.PropertyType.GetGenericArguments()[0];
            else
                SetError(Metadata.AlgoPropertyErrors.OutputIsNotDataSeries);

            this.DefaultThickness = attribute.DefaultThickness;
            this.DefaultColor = attribute.DefaultColor;
            this.DefaultLineStyle = attribute.DefaultLineStyle;
            this.PlotType = attribute.PlotType;

            InitDisplayName(attribute.DisplayName);
        }

        public Type DatdaSeriesBaseType { get; private set; }
        public string DataSeriesBaseTypeFullName { get { return DatdaSeriesBaseType.FullName; } }
        public bool IsShortDefinition { get; private set; }
        public double DefaultThickness { get; private set; }
        public Colors DefaultColor { get; private set; }
        public LineStyles DefaultLineStyle { get; private set; }
        public PlotType PlotType { get; private set; }
        public override AlgoPropertyTypes PropertyType { get { return AlgoPropertyTypes.OutputSeries; } }

        internal OutputDataSeries<T> CreateOutput<T>()
        {
            if (typeof(T) == typeof(double) && IsShortDefinition)
                return (OutputDataSeries<T>)(object)new OutputDataSeries();
            else
                return new OutputDataSeries<T>();
        }

        internal DataSeriesProxy<T> CreateOutput2<T>()
        {
            if (typeof(T) == typeof(double) && IsShortDefinition)
                return (DataSeriesProxy<T>)(object)new DataSeriesProxy();
            else
                return new DataSeriesProxy<T>();
        }
    }
}
