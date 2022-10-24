using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using TickTrader.Algo.Domain;

namespace TickTrader.BotAgent.WebAdmin.Server.Services.Notification
{
    internal sealed class NotificationBuilder
    {
        private const string ServerHeader = "AlgoServer";

        private enum Type { Plugin, Server, Monitoring };

        private readonly ConcurrentDictionary<(Type, string), MessageGroup> _groups = new();
        private readonly StringBuilder _sb = new(1 << 10);


        public bool IsEmpty => _groups.IsEmpty;


        public void AddAlert(AlertRecordInfo alert)
        {
            var key = GetKey(alert);

            if (!_groups.ContainsKey(key))
                _groups.TryAdd(key, new MessageGroup());

            _groups[key].AddRecord(alert);
        }


        public string GetMessage()
        {
            _sb.Clear();

            foreach (((_, var header), var value) in _groups)
            {
                _sb.AppendLine($"*{header}:*"); //bold message
                _sb.AppendLine(value.GetState().EscapeMarkdownV2());
            }

            _groups.Clear();

            return _sb.ToString();
        }


        private static (Type, string) GetKey(AlertRecordInfo alert)
        {
            if (alert.PluginId is not null)
                return (Type.Plugin, alert.PluginId);
            else
                return (Type.Server, ServerHeader);
        }
    }


    internal sealed class MessageGroup
    {
        private readonly static string _space = new(' ', 4);

        private const int MaxListSize = 5;

        private readonly LinkedList<string> _records = new();
        private readonly StringBuilder _builder = new(1 << 8);
        private readonly object _lock = new();

        internal int Count { get; private set; }


        internal void AddRecord(AlertRecordInfo alert)
        {
            lock (_lock)
            {
                Count++;

                _records.AddLast(alert.Message);

                if (_records.Count > MaxListSize)
                    _records.RemoveFirst();
            }
        }

        internal string GetState()
        {
            lock (_lock)
            {
                _builder.Clear();

                foreach (var r in _records)
                    _builder.AppendLine($"{_space}{r}");

                if (Count > MaxListSize)
                    _builder.AppendLine($"{_space}(and {Count - _records.Count} more alerts)");

                Count = 0;

                _records.Clear();

                return _builder.ToString();
            }
        }
    }
}