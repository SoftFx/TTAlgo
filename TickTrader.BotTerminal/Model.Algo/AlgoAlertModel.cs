using Google.Protobuf.WellKnownTypes;
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

        private readonly object _locker = new();
        private readonly bool _isRemoteAlert;

        private List<AlertUpdate> _buffer = new();

        private Timer _timer;

        private bool _newAlerts;


        public string Name { get; }

        public event Action<IEnumerable<AlertUpdate>, bool> AlertUpdateEvent;


        public AlertManagerModel(string name, RemoteAlgoAgent remoteAgent = null)
        {
            _timer = new Timer(SendAlerts, null, 0, AlertsUpdateTimeout);
            _isRemoteAlert = remoteAgent is not null;

            Name = name;
        }


        public void AddAlert(string instanceId, PluginLogRecord record)
        {
            lock (_locker)
            {
                _buffer.Add(new AlertUpdate(instanceId, Name, record.TimeUtc, record.Message));
                _newAlerts = true;
            }
        }

        public void AddAlert(AlertRecordInfo record)
        {
            lock (_locker)
            {
                _buffer.Add(new AlertUpdate(record.PluginId, Name, record.TimeUtc, record.Message));
                _newAlerts = true;
            }
        }

        public void AddAlerts(List<AlertUpdate> records)
        {
            lock (_locker)
            {
                if (records.Count > 0)
                {
                    AlertUpdateEvent?.Invoke(records, _isRemoteAlert);
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
                    AlertUpdateEvent?.Invoke(_buffer, _isRemoteAlert);
                    _buffer = new List<AlertUpdate>();
                    _newAlerts = false;
                }
            }
        }

        private AlertUpdate Convert(AlertRecordInfo rec) => new(rec.PluginId, Name, rec.TimeUtc, rec.Message);


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
        public TimeKey Time { get; }

        public string InstanceId { get; }

        public string AgentName { get; }

        public string Message { get; }


        public AlertUpdate(string id, string agent, Timestamp time, string message) : this(id, agent, message)
        {
            Time = new TimeKey(time);
        }

        public AlertUpdate(string id, string agent, string message)
        {
            Time = new TimeKey(DateTime.MinValue, 0);
            InstanceId = id;
            AgentName = agent;
            Message = message;
        }

        public override string ToString() => $"{InstanceId} -> {Message} ";
    }
}
