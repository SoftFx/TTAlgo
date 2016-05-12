using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Core.Metadata;

namespace TickTrader.Algo.Core.Setup
{
    public abstract class PluginSetup
    {
        private ISetupMetadata metadata;

        private Dictionary<string, PropertySetupBase> properteis = new Dictionary<string, PropertySetupBase>();

        public PluginSetup(AlgoPluginDescriptor descriptor, ISetupMetadata metadata)
        {
            this.Descriptor = descriptor;
            this.metadata = metadata;

            properteis = new Dictionary<string, PropertySetupBase>();

            foreach (var paramDescriptor in descriptor.Parameters)
                properteis[paramDescriptor.Id] = new ParameterSetup(paramDescriptor);

            foreach (var inputDescriptor in descriptor.Inputs)
            {
                var inputSetup = CreateInputSetup(inputDescriptor);
                if (inputSetup != null)
                    properteis[inputDescriptor.Id] = inputSetup;
                else
                    properteis[inputDescriptor.Id] = new UnsupportedProperty(inputDescriptor);
            }

            foreach (var prop in properteis.Values)
            {
                prop.Metadata = metadata;
                prop.Init();
            }
        }

        public AlgoPluginDescriptor Descriptor { get; private set; }
        public abstract PluginFeedBase BaseEntity { get; }

        protected abstract InputSetup CreateInputSetup(InputDescriptor descriptor);

        public virtual IndicatorBuilder CreateIndicatorBuilder()
        {
            IndicatorBuilder builder = new IndicatorBuilder(Descriptor);
            builder.MainSymbol = metadata.MainSymbol;
            Apply(builder);
            return builder;
        }

        //public virtual BotBuilder CreateBotExecutor()
        //{
        //    BotBuilder executor = new BotBuilder(Descriptor);
        //    executor.MainSymbol = metadata.MainSymbol;
        //    Apply(executor);
        //    return executor;
        //}

        public virtual void Apply(PluginBuilder executor)
        {
            foreach (var prop in properteis.Values)
                prop.Apply(executor);
        }

        public T Prop<T>(string proprName) where T : PropertySetupBase
        {
            return (T)properteis[proprName];
        }

        public void SetParam(string name, object value)
        {
            PropertySetupBase prop;
            if (properteis.TryGetValue(name, out prop) && prop is ParameterSetup)
                ((ParameterSetup)prop).Value = value;
            else
                throw new Exception("Parameter Not Found: " + name);
        }

        public Serialization.PluginSetup Serialize()
        {
            var setupObj = new Serialization.PluginSetup();
            setupObj.Properties = properteis.Values.Select(p => p.Serialize()).ToList();
            return setupObj;
        }

        public void Deserialize(Serialization.PluginSetup setupObj)
        {
            if (setupObj.Properties != null)
            {
                foreach (var propObj in setupObj.Properties)
                {
                    if (propObj != null && !string.IsNullOrEmpty(propObj.Name))
                    {
                        PropertySetupBase property;
                        if (properteis.TryGetValue(propObj.Name, out property))
                            property.Deserialize(propObj);
                    }
                }
            }
        }
    }

    public enum PluginFeedBase { Bars, Quotes }

    public class BarBasedPluginSetup : PluginSetup
    {
        public BarBasedPluginSetup(AlgoPluginDescriptor descriptor, ISetupMetadata metadata)
            : base(descriptor, metadata)
        {
        }

        public override PluginFeedBase BaseEntity { get { return PluginFeedBase.Bars; } }

        protected override InputSetup CreateInputSetup(InputDescriptor descriptor)
        {
            switch (descriptor.DataSeriesBaseTypeFullName)
            {
                case "System.Double": return new BarToDoubleInput(descriptor);
                case "TickTrader.Algo.Api.Bar": return new BarInput(descriptor);
                default: return null;
            }
        }
    }

    public class QuoteBasedPluginSetup : PluginSetup
    {
        public QuoteBasedPluginSetup(AlgoPluginDescriptor descriptor, ISetupMetadata metadata)
            : base(descriptor, metadata)
        {
        }

        public override PluginFeedBase BaseEntity { get { return PluginFeedBase.Quotes; } }

        protected override InputSetup CreateInputSetup(InputDescriptor descriptor)
        {
            return null;
        }
    }
}
