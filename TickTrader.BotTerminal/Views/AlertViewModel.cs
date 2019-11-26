using Caliburn.Micro;
using Machinarium.Qnil;
using NLog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace TickTrader.BotTerminal
{
    internal class AlertViewModel : Screen, IWindowModel
    {
        private const string DefaultFilterValue = "All";
        private const int MaxBufferSize = 500;

        private static readonly Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly object _locker = new object();

        private ObservableCircularList<IAlertUpdateEventArgs> _alertBuffer = new ObservableCircularList<IAlertUpdateEventArgs>();
        private WindowManager _wnd;

        private string _selectAgentFilter = DefaultFilterValue;
        private string _selectBotFilter = DefaultFilterValue;


        internal AlertViewModel(WindowManager wnd)
        {
            _wnd = wnd;

            ObservableBuffer = CollectionViewSource.GetDefaultView(_alertBuffer);
            ObservableBuffer.SortDescriptions.Add(new SortDescription { PropertyName = "Time", Direction = ListSortDirection.Descending });
            ObservableBuffer.Filter += new Predicate<object>(AgentAndBotFilter);
            DisplayName = "Alert";
        }

        public ObservableSet<string> AgentsNames { get; } = new ObservableSet<string>() { DefaultFilterValue };

        public ObservableSet<string> BotsNames { get; } = new ObservableSet<string>() { DefaultFilterValue };

        public string SelectAgentNameFilter
        {
            get => _selectAgentFilter;
            set
            {
                if (value == _selectAgentFilter)
                    return;

                _selectAgentFilter = value;
                ObservableBuffer.Refresh();
            }
        }

        public string SelectBotNameFilter
        {
            get => _selectBotFilter;
            set
            {
                if (value == _selectBotFilter)
                    return;

                _selectBotFilter = value;
                ObservableBuffer.Refresh();
            }
        }

        public ICollectionView ObservableBuffer { get; }

        public void SubscribeToModel(IAlertModel model)
        {
            model.AlertUpdateEvent += GetAlertsArray;
        }

        public void SubscribeToModels(IEnumerable<IAlertModel> models)
        {
            foreach (var m in models)
                SubscribeToModel(m);
        }

        public void ShowInFolder()
        {
            Process.Start(EnvService.Instance.LogFolder);
        }

        public void Clear()
        {
            lock (_locker)
            {
                var items = new IAlertUpdateEventArgs[_alertBuffer.Count];
                int position = -1;

                while (_alertBuffer.Count > 0)
                    items[++position] = _alertBuffer.Dequeue();

                position = -1;

                foreach (var item in items)
                    if (!AgentAndBotFilter(item))
                        _alertBuffer.Add(item);
            }
        }

        public void Ok()
        {
            TryClose();
        }

        private bool AgentAndBotFilter(object sender)
        {
            if (!(sender is IAlertUpdateEventArgs item))
                return false;

            return FilterPass(SelectAgentNameFilter, item.AgentName) && FilterPass(SelectBotNameFilter, item.InstanceId);
        }

        private bool FilterPass(string filter, string value)
        {
            return filter == DefaultFilterValue || filter == value;
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
                    _logger.Info(a);
            }
        }

        private void AddRecord(IAlertUpdateEventArgs record)
        {
            while (_alertBuffer.Count > MaxBufferSize)
                _alertBuffer.Dequeue();

            _alertBuffer.Add(record);

            AgentsNames.Add(record.AgentName);
            BotsNames.Add(record.InstanceId);
        }
    }
}
