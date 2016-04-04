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
        }

        public string DataType { get; private set; }
        public object DefaultValue { get; private set; }
        public override AlgoPropertyTypes PropertyType { get { return AlgoPropertyTypes.Parameter; } }
    }
}
