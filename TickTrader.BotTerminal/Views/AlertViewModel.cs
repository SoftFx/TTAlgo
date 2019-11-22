using Caliburn.Micro;
using Machinarium.Qnil;
using NLog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace TickTrader.BotTerminal
{
    internal class AlertViewModel : Screen, IWindowModel
    {
        private static readonly Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private const int MaxBufferSize = 500;

        private readonly object _locker = new object();

        private ObservableCircularList<IAlertUpdateEventArgs> _alertBuffer = new ObservableCircularList<IAlertUpdateEventArgs>();

        private WindowManager _wnd;


        internal AlertViewModel(WindowManager wnd)
        {
            _wnd = wnd;

            ObservabeBuffer = CollectionViewSource.GetDefaultView(_alertBuffer);
            ObservabeBuffer.SortDescriptions.Add(new SortDescription { PropertyName = "Time", Direction = ListSortDirection.Descending });
            DisplayName = "Alert";
        }

        public ICollectionView ObservabeBuffer { get; }

        public void SubscribeToModel(IAlertModel model)
        {
            model.AlertUpdateEvent += GetAlertsArray;
        }

        public void SubscribeToModels(IEnumerable<IAlertModel> models)
        {
            foreach (var m in models)
                SubscribeToModel(m);
        }

        public void Clear()
        {
            _alertBuffer.Clear();
        }

        public void Ok()
        {
            TryClose();
        }

        private void GetAlertsArray(IEnumerable<IAlertUpdateEventArgs> args)
        {
            lock (_locker)
            {
                var record = new List<IAlertUpdateEventArgs>(args);

                Execute.BeginOnUIThread(() =>
                {
                    _wnd.OpenMdiWindow(this);

                    foreach (var a in record)
                        AddRecord(a);
                });

                foreach (var a in record)
                    _logger.Info(a); ;
            }
        }

        private void AddRecord(IAlertUpdateEventArgs record)
        {
            while (_alertBuffer.Count > MaxBufferSize)
                _alertBuffer.Dequeue();

            _alertBuffer.Add(record);
        }
    }
}
