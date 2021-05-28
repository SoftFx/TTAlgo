using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace TickTrader.BotTerminal
{
    public class DragSource : PropertyChangedBase
    {
        private FrameworkElement _associatedElement;
        private object _dataSource;
        private DropState _dropState;

        public DragSource(FrameworkElement associatedElement, object data)
        {
            _associatedElement = associatedElement;
            _dataSource = data;
        }

        public FrameworkElement AssociatedElement
        {
            private get { return _associatedElement; }
            set
            {
                _associatedElement = value;
                NotifyOfPropertyChange(nameof(AssociatedElement));
            }
        }

        public object DataSource
        {
            private get { return _dataSource; }
            set
            {
                _dataSource = value;
                NotifyOfPropertyChange(nameof(DataSource));
            }
        }

        public DropState DropState
        {
            get { return _dropState; }
            set
            {
                _dropState = value;
                NotifyOfPropertyChange(nameof(DropState));
            }
        }
    }
}
