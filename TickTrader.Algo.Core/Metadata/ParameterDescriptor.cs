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

        public ParameterDescriptor(AlgoPluginDescriptor classMetadata, PropertyInfo propertyInfo, ParameterAttribute attribute)
            : base(classMetadata, propertyInfo)
        {
            Validate(propertyInfo);

            if (attribute.DefaultValue != null)
            {
                if (attribute.DefaultValue.GetType() != propertyInfo.PropertyType)
                    SetError(AlgoPropertyErrors.DefaultValueTypeMismatch);
                else
                    DefaultValue = attribute.DefaultValue;
            }

            InitDisplayName(attribute.DisplayName);

            this.DataType = propertyInfo.PropertyType.FullName;

            if (propertyInfo.PropertyType.IsEnum)
            {
                IsEnum = true;
                EnumValues = EnumValueDescriptor.MakeList(propertyInfo.PropertyType);

                if (EnumValues.Count == 0)
                    SetError(AlgoPropertyErrors.EmptyEnum);
            }
            else
                EnumValues = emptyEnumValuesList;
        }

        public bool IsEnum { get; private set; }
        public List<EnumValueDescriptor> EnumValues { get; private set; }
        public string DataType { get; private set; }
        public object DefaultValue { get; private set; }
        public override AlgoPropertyTypes PropertyType { get { return AlgoPropertyTypes.Parameter; } }
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
}
