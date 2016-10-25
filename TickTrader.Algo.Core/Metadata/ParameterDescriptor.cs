using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;
using TickTrader.Algo.Core.Lib;

namespace TickTrader.Algo.Core.Metadata
{
    [Serializable]
    public class ParameterDescriptor : AlgoPropertyDescriptor
    {
        private static readonly List<string> emptyEnumValuesList = new List<string>();

        private bool isDefaultValueDirectlyAssignable;

        public ParameterDescriptor(PropertyInfo propertyInfo, ParameterAttribute attribute)
            : base(propertyInfo)
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
                EnumValues = Enum.GetValues(propertyInfo.PropertyType)
                    .Cast<object>().Select(o => o.ToString()).ToList();

                if (EnumValues.Count == 0)
                    SetError(AlgoPropertyErrors.EmptyEnum);

                if (DefaultValue != null && DefaultValue.GetType() == propertyInfo.PropertyType)
                    DefaultValue = DefaultValue.ToString();
                else
                    DefaultValue = null;
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
        public List<string> EnumValues { get; private set; }
        public List<FileFilterEntry> FileFilters { get; private set; }
        public string DataType { get; private set; }
        public object DefaultValue { get; private set; }
        public override AlgoPropertyTypes PropertyType { get { return AlgoPropertyTypes.Parameter; } }

        internal override void Set(AlgoPlugin instance, object value)
        {
            if (IsEnum && value is string)
            {
                ThrowIfNoAccessor();
                var actualValue = Enum.Parse(reflectioInfo.PropertyType, (string)value);
                base.Set(instance, actualValue);
            }
            else
                base.Set(instance, value);
        }

        internal void ResetValue(Api.AlgoPlugin instance)
        {
            if (isDefaultValueDirectlyAssignable)
                Set(instance, DefaultValue);
        }
    }

    //public class EnumValueProxy : CrossDomainObject
    //{
    //    public object Value { get; set; }
    //}

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
