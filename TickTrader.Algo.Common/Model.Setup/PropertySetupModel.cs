using System;
using TickTrader.Algo.Common.Lib;
using TickTrader.Algo.Common.Model.Config;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Metadata;

namespace TickTrader.Algo.Common.Model.Setup
{
    public abstract class PropertySetupModel : ObservableObject
    {
        private ErrorMsgModel _error;


        public string Id { get; private set; }

        public string DisplayName { get; private set; }

        public ErrorMsgModel Error
        {
            get { return _error; }
            set
            {
                _error = value;
                ErrorChanged(this);
                NotifyPropertyChanged(nameof(Error));
                NotifyPropertyChanged(nameof(IsValid));
                NotifyPropertyChanged(nameof(HasError));
            }
        }

        public bool IsValid => _error == null;

        public bool HasError => _error != null;


        public event Action<PropertySetupModel> ErrorChanged = delegate { };


        public abstract void Load(Property srcProperty);

        public abstract Property Save();

        public abstract void Reset();


        public virtual void Apply(IPluginSetupTarget target) { }


        internal void SetMetadata(PropertyMetadataBase descriptor)
        {
            Id = descriptor.Id;
            DisplayName = descriptor.DisplayName;
        }
    }
}
