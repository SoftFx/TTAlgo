using NLog;
using NLog.Config;
using NLog.Layouts;
using NLog.Targets;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Lib;
using ActorSharp;
using System.Text;
using NLog.Targets.Wrappers;

namespace TickTrader.BotAgent.BA.Models
{
    public class AlertStorage : IAlertStorage
    {
        private const int AlertsInStorage = 100000;

        private CircularList<IAlertEntry> _alertStorage = new CircularList<IAlertEntry>();
        private readonly object _lock = new object();

        public bool FullStorage => _alertStorage.Count > AlertsInStorage;

        public AlertStorage()
        {
        }

        public void AddAlert(ILogEntry record, string id)
        {
            lock (_lock)
            {
                if (FullStorage)
                    _alertStorage.Dequeue();

                _alertStorage.Add(new AlertRecord(record, id));
            }
        }

        public void AttachedAlertLogger(LoggingConfiguration config, string botId, string logDirectory, string fileExt, string archiveExt)
        {
            var alertTarget = $"alert-{botId}";

            var alertFile = new FileTarget(alertTarget)
            {
                FileName = Layout.FromString(Path.Combine(logDirectory, $"${{shortdate}}-alert{fileExt}")),
                Layout = Layout.FromString("${longdate} | ${message}"),
                Encoding = Encoding.UTF8,
                ArchiveEvery = FileArchivePeriod.Day,
                ArchiveFileName = Layout.FromString(Path.Combine(logDirectory, $"{{#}}-alert{archiveExt}")),
                ArchiveNumbering = ArchiveNumberingMode.Date,
                EnableArchiveFileCompression = true,
            };

            var alertWrapper = new AsyncTargetWrapper(alertFile)
            {
                Name = alertTarget,
                BatchSize = 20,
                QueueLimit = 100,
                OverflowAction = AsyncTargetWrapperOverflowAction.Block,
            };

            config.AddTarget(alertWrapper);
            config.AddRule(LogLevel.Warn, LogLevel.Warn, alertTarget);
        }

        public Task<List<IAlertEntry>> QueryAlertsAsync(DateTime from, int maxCount)
        {
            return Task.FromResult(_alertStorage.Where(u => u.TimeUtc.Timestamp > from).Take(maxCount).ToList());
        }
    }

    public class AlertRecord : IAlertEntry
    {
        public TimeKey TimeUtc { get; }

        public string Message { get; }

        public string BotId { get; }

        public AlertRecord(ILogEntry log, string id) : this(log.TimeUtc, log.Message, id)
        {
        }

        public AlertRecord(TimeKey time, string message, string id)
        {
            TimeUtc = time;
            Message = message;
            BotId = id;
        }
    }
}
