using System;
using System.Linq;
using System.Reflection;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.Core.Metadata
{
    [Serializable]
    public class ParameterMetadata : PropertyMetadataBase
    {
        private bool _isDefaultValueDirectlyAssignable;


        public ParameterDescriptor Descriptor { get; }

        public object DefaultValue { get; }


        protected override PropertyDescriptor PropDescriptor => Descriptor;


        public ParameterMetadata(PropertyInfo reflectionInfo, ParameterAttribute attribute)
            : base(reflectionInfo)
        {
            Descriptor = new ParameterDescriptor();
            InitDescriptor(reflectionInfo.Name, attribute.DisplayName);

            Validate(reflectionInfo);

            DefaultValue = attribute.DefaultValue;
            // Filter out unknown objects from DefaultValue because they can cause cross-domain serialization problems
            if (DefaultValue != null && !(DefaultValue is string) && !DefaultValue.GetType().IsPrimitive)
                DefaultValue = null;

            if (DefaultValue == null && reflectionInfo.PropertyType.IsValueType && reflectionInfo.PropertyType.IsPrimitive)
                DefaultValue = Activator.CreateInstance(reflectionInfo.PropertyType);

            _isDefaultValueDirectlyAssignable = DefaultValue != null && DefaultValue.GetType() == reflectionInfo.GetType();

            Descriptor.DataType = reflectionInfo.PropertyType.FullName;
            Descriptor.DefaultValue = DefaultValue?.ToString() ?? string.Empty;
            Descriptor.IsRequired = attribute.IsRequired;

            if (reflectionInfo.PropertyType.IsEnum)
            {
                Descriptor.IsEnum = true;
                Descriptor.EnumValues.AddRange(Enum.GetValues(reflectionInfo.PropertyType).Cast<object>().Select(o => o.ToString()));

                if (Descriptor.EnumValues.Count == 0)
                    SetError(AlgoPropertyErrors.EmptyEnum);

                if (DefaultValue == null && Descriptor.EnumValues.Count > 0)
                    Descriptor.DefaultValue = Descriptor.EnumValues[0].ToString();
            }

            var filterEntries = reflectionInfo.GetCustomAttributes<FileFilterAttribute>(false);
            if (filterEntries != null)
                Descriptor.FileFilters.AddRange(filterEntries.Select(e => new FileFilterEntry(e.Name, e.Mask)));
        }


        internal override void Set(AlgoPlugin instance, object value)
        {
            if (Descriptor.IsEnum && value is string)
            {
                ThrowIfNoAccessor();
                var actualValue = Enum.Parse(ReflectionInfo.PropertyType, (string)value);
                base.Set(instance, actualValue);
            }
            else
                base.Set(instance, value);
        }

        internal void ResetValue(AlgoPlugin instance)
        {
            if (_isDefaultValueDirectlyAssignable)
                Set(instance, DefaultValue);
        }
    }
}
