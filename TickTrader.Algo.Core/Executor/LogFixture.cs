using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using TickTrader.Algo.Core.Lib;

namespace TickTrader.Algo.Core
{
    public class LogFixture : CrossDomainObject, IPluginLogger
    {
        private BufferBlock<BotLogRecord> _logBuffer;
        private ActionBlock<BotLogRecord[]> _logSender;
        private IFixtureContext _context;
        private CancellationTokenSource _stopSrc;
        private TimeKeyGenerator _keyGen = new TimeKeyGenerator();

        internal LogFixture(IFixtureContext context)
        {
            _context = context;
        }

        public void Start()
        {
            _context.Builder.Logger = this;

            var bufferOptions = new DataflowBlockOptions() { BoundedCapacity = 30 };
            var senderOptions = new ExecutionDataflowBlockOptions() { BoundedCapacity = 30 };

            _stopSrc = new CancellationTokenSource();
            _logBuffer = new BufferBlock<BotLogRecord>(bufferOptions);
            _logSender = new ActionBlock<BotLogRecord[]>(msgList =>
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

            _logBuffer.BatchLinkTo(_logSender, 30);
        }

        public async Task Stop()
        {
            try
            {
                _logBuffer.Complete();
                await _logBuffer.Completion;
                await Task.Delay(100);
                _stopSrc.Cancel();
                _logSender.Complete();
                await _logSender.Completion;
            }
            catch (Exception ex)
            {
                _context.OnInternalException(ex);
            }
        }

        public event Action<BotLogRecord[]> NewRecords;

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

        public void OnInitialized()
        {
            AddLogRecord(LogSeverities.Info, "Bot initialized");
            AddLogRecord(LogSeverities.Info, $"Plugin version = {_context.Builder.Metadata.Descriptor.Version}");
        }

        public void OnStart()
        {
            AddLogRecord(LogSeverities.Info, "Bot started");
        }

        public void OnStop()
        {
            AddLogRecord(LogSeverities.Info, "Bot stopped");
            AddLogRecord(LogSeverities.Info, $"Plugin version = {_context.Builder.Metadata.Descriptor.Version}");
        }

        public void OnExit()
        {
            AddLogRecord(LogSeverities.Info, "Bot exited");
        }

        public void OnAbort()
        {
            AddLogRecord(LogSeverities.Info, "Bot aborted");
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
                _logBuffer.SendAsync(new BotLogRecord(timeKey, logSeverity, message, errorDetails)).Wait();
            }
            catch (Exception ex)
            {
                _context.OnInternalException(ex);
            }
        }
    }

    [Serializable]
    public class BotLogRecord
    {
        public BotLogRecord(TimeKey time, LogSeverities logSeverity, string message, string errorDetails)
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
        CustomStatus
    }
}
