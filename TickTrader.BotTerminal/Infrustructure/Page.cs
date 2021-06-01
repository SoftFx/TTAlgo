using Caliburn.Micro;
using System.Linq;

namespace TickTrader.BotTerminal
{
    class Page : Screen
    {
        private bool _isVisible = true;

        public bool IsVisible
        {
            get => _isVisible;
            set
            {
                if (_isVisible != value)
                {
                    _isVisible = value;
                    NotifyOfPropertyChange(nameof(IsVisible));

                    if (!_isVisible)
                        TryDeactivate();
                }
            }
        }

        private void TryDeactivate()
        {
            var conductor = Parent as Conductor<Page>.Collection.OneActive;
            if (conductor != null && conductor.ActiveItem == this)
            {
                var switchToPage = conductor.Items.FirstOrDefault(i => i.IsVisible);
                conductor.ActivateItemAsync(switchToPage);
            }
        }
    }
}
