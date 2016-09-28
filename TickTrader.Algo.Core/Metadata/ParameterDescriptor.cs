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
    public class ParameterDescriptor : AlgoPropertyDescriptor
    {
        private static readonly List<EnumValueDescriptor> emptyEnumValuesList = new List<EnumValueDescriptor>();

        private bool isDefaultValueDirectlyAssignable;

        public ParameterDescriptor(AlgoPluginDescriptor classMetadata, PropertyInfo propertyInfo, ParameterAttribute attribute)
            : base(classMetadata, propertyInfo)
        {
            Validate(propertyInfo);

            DefaultValue = attribute.DefaultValue;

            isDefaultValueDirectlyAssignable = DefaultValue != null
                && DefaultValue.GetType() == propertyInfo.PropertyType;

            InitDisplayName(attribute.DisplayName);

            this.DataType = propertyInfo.PropertyType.FullName;

            this.IsRequired = attribute.IsRequired;

            if (propertyInfo.PropertyType.IsEnum)
            {
                IsEnum = true;
                EnumValues = EnumValueDescriptor.MakeList(propertyInfo.PropertyType);

                if (EnumValues.Count == 0)
                    SetError(AlgoPropertyErrors.EmptyEnum);
            }
            else
                EnumValues = emptyEnumValuesList;

            InspectFileFilterAttr(propertyInfo);
        }

        private void InspectFileFilterAttr(PropertyInfo propertyInfo)
        {
            var filterEntries = propertyInfo.GetCustomAttributes<FileFilterAttribute>(false);
            if (filterEntries != null)
                FileFilters = filterEntries.Select(e => new FileFilterEntry(e.Name, e.Mask)).ToList();
            else
                FileFilters = new List<FileFilterEntry>();
        }

        public bool IsEnum { get; private set; }
        public bool IsRequired { get; private set; }
        public List<EnumValueDescriptor> EnumValues { get; private set; }
        public List<FileFilterEntry> FileFilters { get; private set; }
        public string DataType { get; private set; }
        public object DefaultValue { get; private set; }
        public override AlgoPropertyTypes PropertyType { get { return AlgoPropertyTypes.Parameter; } }

        internal void ResetValue(Api.AlgoPlugin instance)
        {
            if (isDefaultValueDirectlyAssignable)
                Set(instance, DefaultValue);
        }
    }

    [Serializable]
    public class EnumValueDescriptor
    {
        public EnumValueDescriptor(string name, object value)
        {
            this.Name = name;
            this.Value = value;
        }

        public object Value { get; private set; }
        public string Name { get; private set; }

        public static List<EnumValueDescriptor> MakeList(Type enumType)
        {
            return Enum.GetValues(enumType)
                .Cast<object>()
                .Select(o => new EnumValueDescriptor(o.ToString(), o))
                .ToList();
        }
    }

    [Serializable]
    public class FileFilterEntry
    {
        public FileFilterEntry(string name, string mask)
        {
            this.FileTypeName = name;
            this.FileMask = mask;
        }

        public string FileTypeName { get; }
        public string FileMask { get; }
    }
}
