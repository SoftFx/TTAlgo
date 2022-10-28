using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using TickTrader.Algo.Domain;

namespace TickTrader.BotTerminal
{
    internal sealed class AlertManagerModel : IDisposable
    {
        private const int AlertsUpdateTimeout = 1000;

        private readonly BotAgentStorageEntry _creds;
        private readonly object _locker = new();

        private readonly bool _isLocalServer = true;

        private List<AlertUpdate> _buffer = new();

        private Timer _timer;
        private bool _newAlerts;


        private bool IsLocalhost => !_isLocalServer && _creds.IsLocalhost;


        public string Name { get; }

        public event Action<IEnumerable<AlertUpdate>> AlertUpdateEvent;


        public AlertManagerModel(BotAgentStorageEntry creds) : this(creds.Name)
        {
            _isLocalServer = false;
            _creds = creds;
        }

        public AlertManagerModel(string name)
        {
            _timer = new Timer(SendAlerts, null, 0, AlertsUpdateTimeout);

            Name = name;
        }


        public void AddAlert(AlertRecordInfo record)
        {
            lock (_locker)
            {
                _buffer.Add(Convert(record));
                _newAlerts = true;
            }
        }

        public void AddAlerts(List<AlertUpdate> records)
        {
            lock (_locker)
            {
                if (records.Count > 0)
                {
                    AlertUpdateEvent?.Invoke(records);
                }
            }
        }

        public void UpdateRemoteAlert(List<AlertRecordInfo> alerts)
        {
            if (alerts.Count > 0)
                AddAlerts(alerts.Select(Convert).ToList());
        }


        private void SendAlerts(object obj)
        {
            if (_newAlerts)
            {
                lock (_locker)
                {
                    AlertUpdateEvent?.Invoke(_buffer);
                    _buffer = new List<AlertUpdate>();
                    _newAlerts = false;
                }
            }
        }

        private AlertUpdate Convert(AlertRecordInfo rec)
        {
            return new()
            {
                InstanceId = rec.PluginId,
                AgentName = Name,
                Time = new TimeKey(rec.TimeUtc),
                Message = rec.Message,
                SaveToFile = _isLocalServer ? rec.Type == AlertRecordInfo.Types.AlertType.Server : !IsLocalhost,
            };
        }

        public void Dispose()
        {
            if (_timer != null)
            {
                _timer.Dispose();
                _timer = null;
            }
        }
    }


    public sealed record class AlertUpdate
    {
        public TimeKey Time { get; init; } = new TimeKey(DateTime.MinValue, 0);

        public string InstanceId { get; init; }

        public string AgentName { get; init; }

        public string Message { get; init; }

        public bool SaveToFile { get; init; }


        public override string ToString()
        {
            return $"{Time.Timestamp.ToUniversalTime():dd/MM/yyyy HH:mm:ss.fff} AlgoServer: {AgentName} | {InstanceId} -> {Message}";
        }
    }
}
