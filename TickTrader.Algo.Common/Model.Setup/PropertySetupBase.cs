using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Common.Lib;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Metadata;

namespace TickTrader.Algo.Common.Model.Setup
{
    [DataContract(Name = "property", Namespace = "")]
    [KnownType(typeof(IntParamSetup))]
    [KnownType(typeof(DoubleParamSetup))]
    [KnownType(typeof(StringParamSetup))]
    [KnownType(typeof(EnumParamSetup))]
    public abstract class PropertySetupBase : ObservableObject
    {
        private GuiModelMsg error;

        internal void SetMetadata(AlgoPropertyDescriptor descriptor)
        {
            this.Descriptor = descriptor;
            this.Id = descriptor.Id;
        }

        public AlgoPropertyDescriptor Descriptor { get; private set; }
        public bool IsValid { get { return Error == null; } }
        public string DisplayName { get { return Descriptor.DisplayName; } }
        [DataMember(Name = "key")]
        public string Id { get; private set; }
        public bool HasError { get { return this.error != null; } }

        public event Action<PropertySetupBase> ErrorChanged = delegate { };

        public abstract void CopyFrom(PropertySetupBase srcProperty);
        public abstract void Reset();

        public virtual void Apply(IPluginSetupTarget target) { }

        public GuiModelMsg Error
        {
            get { return error; }
            set
            {
                this.error = value;
                ErrorChanged(this);
                NotifyPropertyChanged("IsValid");
                NotifyPropertyChanged("Error");
                NotifyPropertyChanged("HasError");
            }
        }
    }

    public class NullProperty : PropertySetupBase
    {
        public override void CopyFrom(PropertySetupBase srcProperty) { }
        public override void Reset() { }
    }
}
