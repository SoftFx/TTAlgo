using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TickTrader.Algo.Common.Info;
using TickTrader.Algo.Core;

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
        private DateTime _lastAlertTimeUtc;
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

        public void AddAlert(string instanceId, PluginLogRecord record)
        {
            lock (_locker)
            {
                _buffer.Add(new AlertUpdateEventArgsImpl(GetFullInstance(instanceId), record.Time.Timestamp, record.Message));
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
                _lastAlertTimeUtc = new DateTime(1980, 1, 1, 0, 0, 0, DateTimeKind.Utc);
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
                    _lastAlertTimeUtc = alerts.Max(l => l.TimeUtc).Timestamp;
                    AddAlerts(alerts.Select(Convert).ToList<IAlertUpdateEventArgs>());
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Failed to get alerts at {Name}");
            }

            _alertTimer?.Change(AlertsUpdateTimeout, -1);
        }

        private string GetFullInstance(string bot) => $"{bot} ({Name})"; // split into separate parts

        private AlertUpdateEventArgsImpl Convert(BotMessage mes) => new AlertUpdateEventArgsImpl(GetFullInstance(mes.Bot), mes.TimeKey.Timestamp, mes.Message);

        private AlertUpdateEventArgsImpl Convert(AlertRecordInfo rec) => new AlertUpdateEventArgsImpl(GetFullInstance(rec.BotId), rec.TimeUtc.Timestamp, rec.Message);

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

        void AddAlert(string instanceId, PluginLogRecord record);

        void AddAlerts(List<IAlertUpdateEventArgs> records);
    }

    public class AlertUpdateEventArgsImpl : IAlertUpdateEventArgs
    {
        public DateTime Time { get; }

        public string InstanceId { get; }

        public string Message { get; }


        public AlertUpdateEventArgsImpl(string id, DateTime time, string message) : this(id, message)
        {
            Time = time;
        }

        public AlertUpdateEventArgsImpl(string id, string message)
        {
            Time = DateTime.MinValue;
            InstanceId = id;
            Message = message;
        }

        public override string ToString()
        {
            return $"Create: {Time.ToString("yyyy-MM-dd HH:mm:ss.fff")} | {InstanceId} -> {Message} ";
        }
    }

    public interface IAlertUpdateEventArgs
    {
        DateTime Time { get; }

        string InstanceId { get; }

        string Message { get; }
    }
}
