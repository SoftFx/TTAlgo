using Caliburn.Micro;
using Machinarium.Qnil;

namespace TickTrader.BotTerminal
{
    internal class ObjectListViewModel : Screen, IWindowModel
    {
        private readonly DrawableObjectObserver _objectObserver;

        private DrawableObjectViewModel _selectedObject;


        public IObservableList<DrawableObjectViewModel> DrawableObjects { get; }

        public DrawableObjectViewModel SelectedObject
        {
            get => _selectedObject;
            set
            {
                if (_selectedObject == value)
                    return;

                _selectedObject = value;
                NotifyOfPropertyChange(nameof(SelectedObject));
            }
        }


        public ObjectListViewModel(DrawableObjectObserver objectObserver)
        {
            _objectObserver = objectObserver;

            DrawableObjects = objectObserver.DrawableObjects;

            var chartInfo = _objectObserver.ChartHost.Info;
            DisplayName = $"{chartInfo.Symbol}, {chartInfo.Timeframe} Object List";
        }
    }
}
