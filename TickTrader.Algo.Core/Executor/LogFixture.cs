using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Core.Metadata;

namespace TickTrader.Algo.Core
{
    public class LogFixture : CrossDomainObject, IPluginLogger
    {
        private BufferBlock<PluginLogRecord> _logBuffer;
        private ActionBlock<PluginLogRecord[]> _logSender;
        private IFixtureContext _context;
        private TimeKeyGenerator _keyGen = new TimeKeyGenerator();
        private Task _batchLinkTask;
        private AlgoTypes _type;

        internal LogFixture(IFixtureContext context, AlgoTypes type)
        {
            _context = context;
            _type = type;
        }

        public void Start()
        {
            _context.Builder.Logger = this;

            if (!_context.IsGlobalUpdateMarshalingEnabled)
            {
                var bufferOptions = new DataflowBlockOptions() { BoundedCapacity = 30 };
                var senderOptions = new ExecutionDataflowBlockOptions() { BoundedCapacity = 30 };

                _logBuffer = new BufferBlock<PluginLogRecord>(bufferOptions);
                _logSender = new ActionBlock<PluginLogRecord[]>(msgList =>
                {
                    try
                    {
                        NewRecords?.Invoke(msgList);
                    }
                    catch (Exception ex)
                    {
                        _context.OnInternalException(ex);
                    }
                }, senderOptions);

                _batchLinkTask = _logBuffer.BatchLinkTo(_logSender, 30);
            }
        }

        public async Task Stop()
        {
            if (!_context.IsGlobalUpdateMarshalingEnabled)
            {
                try
                {
                    _logBuffer.Complete();
                    await _logBuffer.Completion;
                    await _batchLinkTask;
                    _logSender.Complete();
                    await _logSender.Completion;
                }
                catch (Exception ex)
                {
                    _context.OnInternalException(ex);
                }
            }
        }

        public event Action<PluginLogRecord[]> NewRecords;

        public void OnPrint(string entry)
        {
            AddLogRecord(LogSeverities.Custom, entry);
        }

        public void OnPrint(string entry, params object[] parameters)
        {
            string msg = entry;
            try
            {
                msg = string.Format(entry, parameters);
            }
            catch { }

            AddLogRecord(LogSeverities.Custom, msg);
        }

        public void OnError(Exception ex)
        {
            AddLogRecord(LogSeverities.Error, ex.Message, ex.ToString());
        }

        public void OnError(string message, Exception ex)
        {
            AddLogRecord(LogSeverities.Error, message, ex.ToString());
        }

        public void OnError(string message)
        {
            AddLogRecord(LogSeverities.Error, message);
        }

        public void OnPrintError(string entry)
        {
            AddLogRecord(LogSeverities.Error, entry);
        }

        public void OnPrintError(string entry, params object[] parameters)
        {
            string msg = entry;
            try
            {
                msg = string.Format(entry, parameters);
            }
            catch { }

            AddLogRecord(LogSeverities.Error, msg);
        }

        public void OnPrintInfo(string info)
        {
            AddLogRecord(LogSeverities.Info, info);
        }

        public void OnPrintTrade(string entry)
        {
            AddLogRecord(LogSeverities.Trade, entry);
        }

        public void OnPrintTradeSuccess(string entry)
        {
            AddLogRecord(LogSeverities.TradeSuccess, entry);
        }

        public void OnPrintTradeFail(string entry)
        {
            AddLogRecord(LogSeverities.TradeFail, entry);
        }

        public void OnPrintAlert(string entry)
        {
            AddLogRecord(LogSeverities.Alert, entry);
        }

        public void OnClearAlert()
        {
            AddLogRecord(LogSeverities.AlertClear, "Alerts cleared");
        }

        public void OnInitialized()
        {
            AddLogRecord(LogSeverities.Info, $"{GetPluginType()} initialized");
            AddLogRecord(LogSeverities.Info, $"Plugin version = {_context.Builder.Metadata.Descriptor.Version}");
        }

        public void OnStart()
        {
            AddLogRecord(LogSeverities.Info, $"{GetPluginType()} started");
        }

        public void OnStop()
        {
            AddLogRecord(LogSeverities.Info, $"{GetPluginType()} stopped");
            AddLogRecord(LogSeverities.Info, $"Plugin version = {_context.Builder.Metadata.Descriptor.Version}");
        }

        public void OnExit()
        {
            AddLogRecord(LogSeverities.Info, $"{GetPluginType()} exited");
        }

        public void OnAbort()
        {
            AddLogRecord(LogSeverities.Info, $"{GetPluginType()} aborted");
            AddLogRecord(LogSeverities.Info, $"Plugin version = {_context.Builder.Metadata.Descriptor.Version}");
        }

        public void OnConnected()
        {
            AddLogRecord(LogSeverities.Info, "Connection restored.");
        }

        public void OnDisconnected()
        {
            AddLogRecord(LogSeverities.Error, "Connection lost!");
        }

        public void OnConnectionInfo(string connectionInfo)
        {
            AddLogRecord(LogSeverities.Info, $"Connected to {connectionInfo}");
        }

        public void UpdateStatus(string status)
        {
            AddLogRecord(LogSeverities.CustomStatus, status);
        }

        private void AddLogRecord(LogSeverities logSeverity, string message, string errorDetails = null)
        {
            try
            {
                var timeKey = _keyGen.NextKey(DateTime.Now);
                var record = new PluginLogRecord(timeKey, logSeverity, message, errorDetails);
                if (_logBuffer != null)
                    _logBuffer.SendAsync(record).Wait();
                else
                    _context.SendExtUpdate(record);
            }
            catch (Exception ex)
            {
                _context.OnInternalException(ex);
            }
        }

        private string GetPluginType()
        {
            switch (_type)
            {
                case AlgoTypes.Robot: return "Bot";
                case AlgoTypes.Indicator: return "Indicator";
                default: return "Unknow plugin";
            }
        }
    }

    [Serializable]
    public class PluginLogRecord
    {
        public PluginLogRecord(TimeKey time, LogSeverities logSeverity, string message, string errorDetails)
        {
            Time = time;
            Severity = logSeverity;
            Message = message;
            Details = errorDetails;
        }

        public TimeKey Time { get; set; }
        public LogSeverities Severity { get; set; }
        public string Message { get; set; }
        public string Details { get; set; }
    }

    public enum LogSeverities
    {
        Info,
        Error,
        Trade,
        TradeSuccess,
        TradeFail,
        Custom,
        CustomStatus,
        Alert,
        AlertClear,
    }
}
