using Google.Protobuf.WellKnownTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TickTrader.Algo.Common.Info;
using TickTrader.Algo.Core;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Protocol;

namespace TickTrader.BotTerminal
{
    internal class AlgoAlertModel : IAlertModel, IDisposable
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private const int AlertsUpdateTimeout = 1000;

        private readonly Timer _timer;
        private readonly object _locker = new object();

        private List<IAlertUpdateEventArgs> _buffer = new List<IAlertUpdateEventArgs>();
        private bool _newAlerts = false;

        private RemoteAlgoAgent _remoteAgent;
        private Timestamp _lastAlertTimeUtc;
        private Timer _alertTimer;

        public AlgoAlertModel(string name, RemoteAlgoAgent remoteAgent = null)
        {
            Name = name;

            _remoteAgent = remoteAgent;
            _timer = new Timer(SendAlerts, null, 0, AlertsUpdateTimeout);

            SubscribeToRemoteAgent();
        }

        public string Name { get; }

        public event Action<IEnumerable<IAlertUpdateEventArgs>> AlertUpdateEvent;

        public void AddAlert(string instanceId, UnitLogRecord record)
        {
            lock (_locker)
            {
                _buffer.Add(new AlertUpdateEventArgsImpl(instanceId, Name, record.TimeUtc, record.Message, _remoteAgent != null));
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

        private void SubscribeToRemoteAgent()
        {
            if (_remoteAgent == null)
                return;

            if (_alertTimer == null)
            {
                _lastAlertTimeUtc = new Timestamp(); // 1970-01-01
                _alertTimer = new Timer(UpdateRemoteAlert, null, AlertsUpdateTimeout, -1);
            }
        }

        private async void UpdateRemoteAlert(object state)
        {
            _alertTimer?.Change(-1, -1);

            try
            {
                var alerts = await _remoteAgent.GetAlerts(_lastAlertTimeUtc);
                if (alerts.Length > 0)
                {
                    _lastAlertTimeUtc = alerts.Max(l => l.TimeUtc);
                    AddAlerts(alerts.Select(Convert).ToList<IAlertUpdateEventArgs>());
                }
            }
            catch (BAException baex)
            {
                _logger.Error($"Failed to get alerts at {Name}: {baex.Message}");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Failed to get alerts at {Name}");
            }

            _alertTimer?.Change(AlertsUpdateTimeout, -1);
        }

        private AlertUpdateEventArgsImpl Convert(AlertRecordInfo rec) => new AlertUpdateEventArgsImpl(rec.BotId, Name, rec.TimeUtc, rec.Message, _remoteAgent != null);

        public void Dispose() //add dispose storage to remote agent
        {
            if (_alertTimer != null)
            {
                _alertTimer.Dispose();
                _alertTimer = null;
            }
        }
    }

    internal interface IAlertModel
    {
        string Name { get; }

        event Action<IEnumerable<IAlertUpdateEventArgs>> AlertUpdateEvent;

        void AddAlert(string instanceId, UnitLogRecord record);

        void AddAlerts(List<IAlertUpdateEventArgs> records);
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
