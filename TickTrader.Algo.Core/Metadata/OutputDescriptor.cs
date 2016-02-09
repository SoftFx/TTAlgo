using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.Core.Metadata
{
    public class OutputDescriptor : AlgoPropertyDescriptor
    {
        public OutputDescriptor(AlgoDescriptor classMetadata, PropertyInfo propertyInfo, object attribute)
            : base(classMetadata, propertyInfo)
        {
            Attribute = (OutputAttribute)attribute;
            Validate();

            var propertyType = this.Info.PropertyType;

            if (propertyType == typeof(DataSeries))
            {
                DatdaSeriesBaseType = typeof(double);
                IsShortDefinition = true;
            }
            else if (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(DataSeries<>))
                DatdaSeriesBaseType = propertyInfo.PropertyType.GetGenericArguments()[0];
            else
                SetError(Metadata.AlgoPropertyErrors.OutputIsNotDataSeries);

            InitDisplayName(Attribute.DisplayName);
        }

        public override AlgoPropertyInfo GetInteropCopy()
        {
            OutputInfo copy = new OutputInfo();
            FillCommonProperties(copy);
            copy.DataSeriesBaseTypeFullName = DatdaSeriesBaseType.FullName;
            return copy;
        }

        public Type DatdaSeriesBaseType { get; private set; }
        public bool IsShortDefinition { get; private set; }
        public OutputAttribute Attribute { get; private set; }
        public override AlgoPropertyTypes PropertyType { get { return AlgoPropertyTypes.OutputSeries; } }

        public OutputDataSeries<T> CreateOutput<T>()
        {
            if (typeof(T) == typeof(double) && IsShortDefinition)
                return (OutputDataSeries<T>)(object)new OutputDataSeries();
            else
                return new OutputDataSeries<T>();
        }
    }
}
