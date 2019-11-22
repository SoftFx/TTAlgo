using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TickTrader.Algo.Core;

namespace TickTrader.BotTerminal
{
    internal class AlgoAlertModel : IAlertModel
    {
        private readonly Timer _timer;
        private readonly object _locker = new object();

        private List<IAlertUpdateEventArgs> _buffer = new List<IAlertUpdateEventArgs>();
        private bool _newAlerts = false;

        public AlgoAlertModel(string name)
        {
            Name = name;

            _timer = new Timer(SendAlerts, null, 0, 1000);
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

        public void AddAlerts(List<BotMessage> records)
        {
            lock (_locker)
            {
                if (records.Count > 0)
                {
                    AlertUpdateEvent?.Invoke(records.Select(Convert));
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

        private string GetFullInstance(string bot) => $"{bot} ({Name})"; // split into separate parts

        private AlertUpdateEventArgsImpl Convert(BotMessage mes) => new AlertUpdateEventArgsImpl(GetFullInstance(mes.Bot), mes.TimeKey.Timestamp, mes.Message);
    }

    internal interface IAlertModel
    {
        string Name { get; }

        event Action<IEnumerable<IAlertUpdateEventArgs>> AlertUpdateEvent;

        void AddAlert(string instanceId, PluginLogRecord record);

        void AddAlerts(List<BotMessage> records);
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
