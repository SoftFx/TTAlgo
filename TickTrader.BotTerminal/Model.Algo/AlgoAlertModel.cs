using Google.Protobuf.WellKnownTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using TickTrader.Algo.Domain;

namespace TickTrader.BotTerminal
{
    internal class AlgoAlertModel : IAlertModel, IDisposable
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private const int AlertsUpdateTimeout = 1000;

        private Timer _timer;
        private readonly object _locker = new object();

        private List<IAlertUpdateEventArgs> _buffer = new List<IAlertUpdateEventArgs>();
        private bool _newAlerts = false;

        private RemoteAlgoAgent _remoteAgent;
        private Timestamp _lastAlertTimeUtc;

        public AlgoAlertModel(string name, RemoteAlgoAgent remoteAgent = null)
        {
            Name = name;

            _lastAlertTimeUtc = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).ToTimestamp();
            _remoteAgent = remoteAgent;
            _timer = new Timer(SendAlerts, null, 0, AlertsUpdateTimeout);
        }

        public string Name { get; }

        public event Action<IEnumerable<IAlertUpdateEventArgs>> AlertUpdateEvent;

        public void AddAlert(string instanceId, PluginLogRecord record)
        {
            lock (_locker)
            {
                _buffer.Add(new AlertUpdateEventArgsImpl(instanceId, Name, record.TimeUtc, record.Message, _remoteAgent != null));
                _newAlerts = true;
            }
        }

        public void AddAlert(AlertRecordInfo record)
        {
            lock (_locker)
            {
                _buffer.Add(new AlertUpdateEventArgsImpl(record.PluginId, Name, record.TimeUtc, record.Message, _remoteAgent != null));
                _newAlerts = true;
            }
        }

        public void AddAlerts(List<IAlertUpdateEventArgs> records)
        {
            lock (_locker)
            {
                if (records.Count > 0)
                {
                    AlertUpdateEvent?.Invoke(records);
                }
            }
        }

        private void SendAlerts(object obj)
        {
            if (_newAlerts)
            {
                lock (_locker)
                {
                    AlertUpdateEvent?.Invoke(_buffer);
                    _buffer = new List<IAlertUpdateEventArgs>();
                    _newAlerts = false;
                }
            }
        }

        public void SubscribeToRemoteAgent()
        {
            if (_remoteAgent == null)
                return;

            //_remoteAgent.SubscribeToAlerts(_lastAlertTimeUtc);
        }

        public void UpdateRemoteAlert(List<AlertRecordInfo> alerts)
        {
            if (alerts.Count > 0)
            {
                AddAlerts(alerts.Select(Convert).ToList<IAlertUpdateEventArgs>());
                _lastAlertTimeUtc = alerts.Max(u => u.TimeUtc);
            }
        }

        private AlertUpdateEventArgsImpl Convert(AlertRecordInfo rec) => new AlertUpdateEventArgsImpl(rec.PluginId, Name, rec.TimeUtc, rec.Message, _remoteAgent != null);

        public void Dispose()
        {
            if (_timer != null)
            {
                _timer.Dispose();
                _timer = null;
            }
        }
    }

    internal interface IAlertModel
    {
        string Name { get; }

        event Action<IEnumerable<IAlertUpdateEventArgs>> AlertUpdateEvent;

        void AddAlert(string instanceId, PluginLogRecord record);

        void AddAlert(AlertRecordInfo record);

        void AddAlerts(List<IAlertUpdateEventArgs> records);

        void UpdateRemoteAlert(List<AlertRecordInfo> alerts);

        void SubscribeToRemoteAgent();
    }

    public class AlertUpdateEventArgsImpl : IAlertUpdateEventArgs
    {
        public TimeKey Time { get; }

        public string InstanceId { get; }

        public string AgentName { get; }

        public string Message { get; }

        public bool IsRemoteAgent { get; }

        public AlertUpdateEventArgsImpl(string id, string agent, Timestamp time, string message, bool isRemote = false) : this(id, agent, message, isRemote)
        {
            Time = new TimeKey(time);
        }

        public AlertUpdateEventArgsImpl(string id, string agent, string message, bool isRemote)
        {
            Time = new TimeKey(DateTime.MinValue, 0);
            InstanceId = id;
            AgentName = agent;
            Message = message;
            IsRemoteAgent = isRemote;
        }

        public override string ToString() => $"{InstanceId} -> {Message} ";
    }

    public interface IAlertUpdateEventArgs
    {
        TimeKey Time { get; }

        string InstanceId { get; }

        string AgentName { get; }

        string Message { get; }

        bool IsRemoteAgent { get; }
    }
}
