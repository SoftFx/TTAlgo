using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.Core.Metadata
{
    public class ParameterDescriptor : AlgoPropertyDescriptor
    {
        public ParameterDescriptor(AlgoDescriptor classMetadata, PropertyInfo propertyInfo, object attribute)
            : base(classMetadata, propertyInfo)
        {
            Attribute = (ParameterAttribute)attribute;
            Validate();

            if (Attribute.DefaultValue != null)
            {
                if (Attribute.DefaultValue.GetType() != propertyInfo.PropertyType)
                    SetError(AlgoPropertyErrors.DefaultValueTypeMismatch);
                else
                    DefaultValue = Attribute.DefaultValue;
            }

            InitDisplayName(Attribute.DisplayName);
        }

        public override AlgoPropertyInfo GetInteropCopy()
        {
            ParameterInfo copy = new ParameterInfo();
            FillCommonProperties(copy);
            copy.DataType = Info.PropertyType.FullName;
            copy.DefaultValue = DefaultValue;
            return copy;
        }

        public ParameterAttribute Attribute { get; private set; }
        public object DefaultValue { get; private set; }
        public override AlgoPropertyTypes PropertyType { get { return AlgoPropertyTypes.Parameter; } }
    }
}
