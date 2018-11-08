using Caliburn.Micro;
using System;
using TickTrader.Algo.Common.Info;
using TickTrader.Algo.Common.Model.Config;
using TickTrader.Algo.Common.Model.Setup;
using TickTrader.Algo.Core.Metadata;

namespace TickTrader.BotTerminal
{
    public abstract class PropertySetupViewModel : PropertyChangedBase
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
                NotifyOfPropertyChange(nameof(Error));
                NotifyOfPropertyChange(nameof(IsValid));
                NotifyOfPropertyChange(nameof(HasError));
            }
        }

        public bool IsValid => _error == null;

        public bool HasError => _error != null;

        public event Action<PropertySetupViewModel> ErrorChanged = delegate { };

        public abstract void Load(Property srcProperty);

        public abstract Property Save();

        public abstract void Reset();

        internal void SetMetadata(PropertyDescriptor descriptor)
        {
            Id = descriptor.Id;
            DisplayName = descriptor.DisplayName;
        }
    }
}
