using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Core.Metadata;

namespace TickTrader.Algo.Core.Setup
{
    public abstract class PropertySetupBase
    {
        public PropertySetupBase()
        {
        }

        public string Id { get { return GetDescriptor().Id; } }
        public ISetupMetadata Metadata { get; internal set; }

        public abstract void Apply(PluginExecutor executor);

        public virtual void Init() { }
        protected abstract AlgoPropertyDescriptor GetDescriptor();

        public abstract Serialization.PropertySetup Serialize();
        public abstract void Deserialize(Serialization.PropertySetup propObj);

        protected string NormalizeSymbol(string inputSmb)
        {
            if (Metadata.SymbolExist(inputSmb))
                return inputSmb;
            return Metadata.MainSymbol;
        }
    }


    public class UnsupportedProperty : PropertySetupBase
    {
        private AlgoPropertyDescriptor descriptor;

        public UnsupportedProperty(AlgoPropertyDescriptor descriptor)
        {
            this.descriptor = descriptor;
        }

        public override void Apply(PluginExecutor executor)
        {
        }

        public override void Deserialize(Serialization.PropertySetup propObj)
        {
        }

        public override Serialization.PropertySetup Serialize()
        {
            return new Serialization.UnsupportedProperty();
        }

        protected override AlgoPropertyDescriptor GetDescriptor()
        {
            return descriptor;
        }
    }
}
