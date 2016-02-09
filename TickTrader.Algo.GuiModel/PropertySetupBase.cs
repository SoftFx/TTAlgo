using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Core.Metadata;

namespace TickTrader.Algo.GuiModel
{
    public abstract class PropertySetupBase : ObservableObject
    {
        private GuiModelMsg error;

        internal void SetMetadata(AlgoPropertyInfo descriptor)
        {
            this.DisplayName = descriptor.DisplayName;
            this.Id = descriptor.Id;
        }

        public bool Valid { get { return Error != null; } }
        public string DisplayName { get; private set; }
        public string Id { get; private set; }
        public bool HasError { get { return this.error != null; } }

        public event Action<PropertySetupBase> ErrorChanged = delegate { };

        public abstract void CopyFrom(PropertySetupBase srcProperty);
        public abstract void Reset();

        public GuiModelMsg Error
        {
            get { return error; }
            set
            {
                this.error = value;
                ErrorChanged(this);
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
