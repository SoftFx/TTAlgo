using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Core.Metadata;
using TickTrader.Algo.Core.Setup;
using TickTrader.Algo.Core.Setup.Serialization;

namespace TickTrader.Algo.Core.Setup
{
    public class ParameterSetup : PropertySetupBase
    {
        public ParameterSetup(ParameterDescriptor descriptor)
        {
            this.Descriptor = descriptor;
            this.Name = descriptor.Id;
            this.DataType = descriptor.DataType;
        }

        public string Name { get; private set; }
        public string DataType { get; private set; }
        public object Value { get; set; }
        public ParameterDescriptor Descriptor { get; private set; }

        protected override AlgoPropertyDescriptor GetDescriptor()
        {
            return Descriptor;
        }

        public override void Init()
        {
            this.Value = Descriptor.DefaultValue;
        }

        public override PropertySetup Serialize()
        {
            return new Serialization.ParameterSetup() { Name = Name, Value = Value };
        }

        public override void Deserialize(PropertySetup propObj)
        {
            var paramProperty = propObj as Serialization.ParameterSetup;
            try
            {
                if (paramProperty != null)
                    this.Value = paramProperty.Value;
            }
            catch (InvalidCastException) { }
        }

        public override void Apply(PluginExecutor executor)
        {
            executor.SetParameter(Name, Value);
        }
    }
}
