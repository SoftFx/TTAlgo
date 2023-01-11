using System;
using System.Threading.Tasks;
using TickTrader.Algo.Core;

namespace TickTrader.Algo.CoreV1
{
    public class LogFixture : IPluginLogger
    {
        private readonly TimeKeyGenerator _keyGen = new TimeKeyGenerator();
        private readonly IFixtureContext _context;
        private readonly bool _isTradeBot, _saveOnDisk;
        private readonly string _logDir;

        private IPluginLogWriter _logWriter;

        internal LogFixture(IFixtureContext context, bool isTradeBot, string logDir, bool saveOnDisk)
        {
            _context = context;
            _isTradeBot = isTradeBot;
            _logDir = logDir;
            _saveOnDisk = saveOnDisk;
        }

        public void Start()
        {
            if (_saveOnDisk)
            {
                _logWriter = PluginLogWriter.Create();
                _logWriter.Start(_logDir);
            }

            _context.Builder.Logger = this;
        }

        public Task Stop()
        {
            if (_logWriter != null)
            {
                _logWriter.Stop();
                _logWriter = null;
            }

            return Task.CompletedTask;
        }

        public void OnPrint(string entry)
        {
            AddLogRecord(Domain.PluginLogRecord.Types.LogSeverity.Custom, entry);
        }

        public void OnPrint(string entry, params object[] parameters)
        {
            string msg = entry;
            try
            {
                msg = string.Format(entry, parameters);
            }
            catch { }

            AddLogRecord(Domain.PluginLogRecord.Types.LogSeverity.Custom, msg);
        }

        public void OnError(Exception ex)
        {
            AddLogRecord(Domain.PluginLogRecord.Types.LogSeverity.Error, ex.Message, ex.ToString());
        }

        public void OnError(string message, Exception ex)
        {
            AddLogRecord(Domain.PluginLogRecord.Types.LogSeverity.Error, message, ex.ToString());
        }

        public void OnError(string message)
        {
            AddLogRecord(Domain.PluginLogRecord.Types.LogSeverity.Error, message);
        }

        public void OnPrintError(string entry)
        {
            AddLogRecord(Domain.PluginLogRecord.Types.LogSeverity.Error, entry);
        }

        public void OnPrintError(string entry, params object[] parameters)
        {
            string msg = entry;
            try
            {
                msg = string.Format(entry, parameters);
            }
            catch { }

            AddLogRecord(Domain.PluginLogRecord.Types.LogSeverity.Error, msg);
        }

        public void OnPrintInfo(string info)
        {
            AddLogRecord(Domain.PluginLogRecord.Types.LogSeverity.Info, info);
        }

        public void OnPrintTrade(string entry)
        {
            AddLogRecord(Domain.PluginLogRecord.Types.LogSeverity.Trade, entry);
        }

        public void OnPrintTradeSuccess(string entry)
        {
            AddLogRecord(Domain.PluginLogRecord.Types.LogSeverity.TradeSuccess, entry);
        }

        public void OnPrintTradeFail(string entry)
        {
            AddLogRecord(Domain.PluginLogRecord.Types.LogSeverity.TradeFail, entry);
        }

        public void OnPrintAlert(string entry)
        {
            AddLogRecord(Domain.PluginLogRecord.Types.LogSeverity.Alert, entry);
        }

        public void OnInitialized()
        {
            if (_isTradeBot)
            {
                AddLogRecord(Domain.PluginLogRecord.Types.LogSeverity.Info, $"Bot initialized");
                AddLogRecord(Domain.PluginLogRecord.Types.LogSeverity.Info, $"Plugin version = {_context.Builder.Metadata.Descriptor.Version}");
            }
        }

        public void OnStart()
        {
            if (_isTradeBot)
            {
                AddLogRecord(Domain.PluginLogRecord.Types.LogSeverity.Info, $"Bot started");
                _logWriter?.OnBotStarted();
            }
            else AddLogRecord(Domain.PluginLogRecord.Types.LogSeverity.Info, $"Indicator started");
        }

        public void OnStop()
        {
            if (_isTradeBot)
            {
                AddLogRecord(Domain.PluginLogRecord.Types.LogSeverity.Info, $"Bot stopped");
                AddLogRecord(Domain.PluginLogRecord.Types.LogSeverity.Info, $"Plugin version = {_context.Builder.Metadata.Descriptor.Version}");
                _logWriter?.OnBotStopped();
            }
            else AddLogRecord(Domain.PluginLogRecord.Types.LogSeverity.Info, $"Indicator stopped");
        }

        public void OnExit()
        {
            if (_isTradeBot)
                AddLogRecord(Domain.PluginLogRecord.Types.LogSeverity.Info, $"Bot exited");
        }

        public void OnAbort()
        {
            if (_isTradeBot)
            {
                AddLogRecord(Domain.PluginLogRecord.Types.LogSeverity.Info, $"Bot aborted");
                AddLogRecord(Domain.PluginLogRecord.Types.LogSeverity.Info, $"Plugin version = {_context.Builder.Metadata.Descriptor.Version}");
                _logWriter?.OnBotStopped();
            }
        }

        public void OnConnected()
        {
            if (_isTradeBot)
                AddLogRecord(Domain.PluginLogRecord.Types.LogSeverity.Info, "Connection restored.");
        }

        public void OnDisconnected()
        {
            if (_isTradeBot)
                AddLogRecord(Domain.PluginLogRecord.Types.LogSeverity.Error, "Connection lost!");
        }

        public void OnConnectionInfo(string connectionInfo)
        {
            if (_isTradeBot)
                AddLogRecord(Domain.PluginLogRecord.Types.LogSeverity.Info, $"Connected to {connectionInfo}");
        }

        public void UpdateStatus(string status)
        {
            if (_isTradeBot)
            {
                try
                {
                    var update = new Domain.PluginStatusUpdate(_context.InstanceId, status);

                    _logWriter?.OnStatusUpdate(update);
                    _context.SendNotification(update);
                }
                catch(Exception ex)
                {
                    _context.OnInternalException(ex);
                }
            }
        }

        private void AddLogRecord(Domain.PluginLogRecord.Types.LogSeverity logSeverity, string message, string errorDetails = null)
        {
            try
            {
                var timeKey = _keyGen.NextKey(DateTime.UtcNow);
                var record = new Domain.PluginLogRecord(timeKey, logSeverity, message, errorDetails);

                _logWriter?.OnLogRecord(record);
                _context.SendNotification(record);
            }
            catch (Exception ex)
            {
                _context.OnInternalException(ex);
            }
        }
    }
}
