using NLog;
using NLog.Config;
using NLog.Layouts;
using NLog.Targets;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TickTrader.Algo.Core.Lib;

namespace TickTrader.BotAgent.BA.Models
{
    public class AlertStorage
    {
        private const int AlertsInStorage = 100000;

        private CircularList<ILogEntry> _alertStorage = new CircularList<ILogEntry>();

        private readonly string _botId, _logDirectory, _fileExt, _archExt;

        public bool FullStorage => _alertStorage.Count > AlertsInStorage;

        public AlertStorage(string botId, string logDirectory, string fileExt, string archExt)
        {
            _botId = botId;
            _logDirectory = logDirectory;
            _fileExt = fileExt;
            _archExt = archExt;
        }

        public void AddAlert(ILogEntry newAlert)
        {
            if (FullStorage)
                _alertStorage.Dequeue();

            _alertStorage.Add(newAlert);
        }

        public void AttachedAlertLogger(LoggingConfiguration config)
        {
            var alertTarget = $"alert-{_botId}";

            var alertFile = new FileTarget(alertTarget)
            {
                FileName = Layout.FromString(Path.Combine(_logDirectory, $"${{shortdate}}-alert{_fileExt}")),
                Layout = Layout.FromString("${longdate} | ${message}"),
                ArchiveEvery = FileArchivePeriod.Day,
                ArchiveFileName = Layout.FromString(Path.Combine(_logDirectory, $"{{#}}-alert{_archExt}")),
                ArchiveNumbering = ArchiveNumberingMode.Date,
                EnableArchiveFileCompression = true,
            };

            config.AddTarget(alertFile);
            config.AddRule(LogLevel.Warn, LogLevel.Warn, alertTarget);
        }

        public List<ILogEntry> QueryAlerts(DateTime from, int maxCount)
        {
            return _alertStorage.Where(u => u.TimeUtc.Timestamp > from).Take(maxCount).ToList();
        }
    }
}
