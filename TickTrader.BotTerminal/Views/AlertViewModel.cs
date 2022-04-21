using Caliburn.Micro;
using Machinarium.Qnil;
using NLog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Data;

namespace TickTrader.BotTerminal
{
    internal class AlertViewModel : Screen
    {
        private const string WindowsId = "Tab_Alert";
        private const string DefaultFilterValue = "All";
        private const int MaxBufferSize = 500;

        private static readonly Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly object _locker = new object();

        private ObservableCircularList<IAlertUpdateEventArgs> _alertBuffer = new ObservableCircularList<IAlertUpdateEventArgs>();
        private LinkedList<IAlertUpdateEventArgs> _lockBuffer = new LinkedList<IAlertUpdateEventArgs>();

        private WindowManager _wnd;
        private IShell _shell;
        private bool _isRegistred = false;
        private bool _updateBuffer = true;

        private string _selectAgentFilter = DefaultFilterValue;
        private string _selectBotFilter = DefaultFilterValue;


        internal AlertViewModel(WindowManager wnd, IShell shell)
        {
            _wnd = wnd;
            _shell = shell;

            ObservableBuffer = CollectionViewSource.GetDefaultView(_alertBuffer);
            ObservableBuffer.SortDescriptions.Add(new SortDescription { PropertyName = "Time", Direction = ListSortDirection.Descending });
            ObservableBuffer.Filter += new Predicate<object>(AgentAndBotFilter);
            DisplayName = "Alert";
        }

        public bool IsOpened => _shell.DockManagerService.IsAlertOpened;

        public ObservableCounter<string> AgentsNames { get; } = new ObservableCounter<string>(DefaultFilterValue);

        public ObservableCounter<string> BotsNames { get; } = new ObservableCounter<string>(DefaultFilterValue);

        public bool UpdateBuffer
        {
            get => _updateBuffer;
            set
            {
                if (value == _updateBuffer)
                    return;

                _updateBuffer = value;

                if (value && _lockBuffer.Count > 0)
                {
                    lock (_locker)
                    {
                        Execute.BeginOnUIThread(() =>
                        {
                            foreach (var m in _lockBuffer)
                                AddRecord(m);

                            _lockBuffer.Clear();
                        });
                    }
                }
            }
        }

        public string SelectAgentNameFilter
        {
            get => _selectAgentFilter;
            set
            {
                if (value == _selectAgentFilter || string.IsNullOrEmpty(value))
                    return;

                _selectAgentFilter = value;
                ObservableBuffer.Refresh();
                NotifyOfPropertyChange(nameof(SelectAgentNameFilter));
            }
        }

        public string SelectBotNameFilter
        {
            get => _selectBotFilter;
            set
            {
                if (value == _selectBotFilter || string.IsNullOrEmpty(value))
                    return;

                _selectBotFilter = value;
                ObservableBuffer.Refresh();
                NotifyOfPropertyChange(nameof(SelectBotNameFilter));
            }
        }

        public ICollectionView ObservableBuffer { get; }

        public void SubscribeToModel(IAlertModel model)
        {
            model.AlertUpdateEvent += GetAlertsArray;
        }

        public void SubcribeToModels(IEnumerable<IAlertModel> models)
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
                try
                {
                    var items = new IAlertUpdateEventArgs[_alertBuffer.Count];
                    int position = -1;

                    while (_alertBuffer.Count > 0)
                        items[++position] = GetAlertItem();

                    position = -1;

                    foreach (var item in items)
                        if (!AgentAndBotFilter(item))
                            SetAlertItem(item);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex);
                }
            }

            SelectAgentNameFilter = DefaultFilterValue;
            SelectBotNameFilter = DefaultFilterValue;
        }

        public void Ok()
        {
            TryCloseAsync();
        }

        public void UpdateBotAgents(DictionaryUpdateArgs<string, BotAgentConnectionManager> args)
        {
            if (args.Action == DLinqAction.Insert)
                SubscribeToModel(args.NewItem.RemoteAgent.AlertModel);
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
                var records = new List<IAlertUpdateEventArgs>(args);

                if (UpdateBuffer)
                {
                    Execute.BeginOnUIThread(() =>
                    {
                        if (_isRegistred && !IsOpened)
                            _shell.DockManagerService.IsAlertOpened = true;

                        foreach (var a in records)
                            AddRecord(a);
                    });
                }
                else
                {
                    foreach (var a in records)
                    {
                        if (_lockBuffer.Count >= MaxBufferSize)
                            _lockBuffer.RemoveFirst();

                        _lockBuffer.AddLast(a);
                    }
                }

                if (records.Count > 0 && !records.First().IsRemoteAgent)
                    foreach (var a in records)
                        _logger.Info(a);
            }
        }

        private void AddRecord(IAlertUpdateEventArgs record)
        {
            while (_alertBuffer.Count > MaxBufferSize)
                GetAlertItem();

            SetAlertItem(record);
        }

        private IAlertUpdateEventArgs GetAlertItem()
        {
            var item = _alertBuffer.Dequeue();
            AgentsNames.Remove(item.AgentName);
            BotsNames.Remove(item.InstanceId);

            return item;
        }

        private void SetAlertItem(IAlertUpdateEventArgs item)
        {
            _alertBuffer.Add(item);

            AgentsNames.Add(item.AgentName);
            BotsNames.Add(item.InstanceId);
        }

        public void RegisterAlertWindow()
        {
            _shell.DockManagerService.RegisterView(this, WindowsId);
            _isRegistred = true;
        }
    }
}
