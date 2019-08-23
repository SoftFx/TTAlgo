using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.BotTerminal
{
    public class OptionalItem<T> : ObservableObject
    {
        private bool _enabled;

        public OptionalItem(T value, bool enabled = true)
        {
            Value = value;
            _enabled = enabled;
        }

        public T Value { get; }

        public bool IsEnabled
        {
            get => _enabled;
            set
            {
                if (_enabled != value)
                {
                    _enabled = value;
                    NotifyOfPropertyChange(nameof(IsEnabled));
                }
            }
        }
    }
}
