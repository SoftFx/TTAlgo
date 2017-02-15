using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Metadata;

namespace TickTrader.Algo.Common.PluginSetup
{
    public abstract class PropertySetupBase : ObservableObject
    {
        private GuiModelMsg error;

        internal void SetMetadata(AlgoPropertyDescriptor descriptor)
        {
            this.Descriptor = descriptor;
        }

        public AlgoPropertyDescriptor Descriptor { get; private set; }
        public bool IsValid { get { return Error == null; } }
        public string DisplayName { get { return Descriptor.DisplayName; } }
        public string Id { get { return Descriptor.Id; } }
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
