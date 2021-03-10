using System;
using System.Threading.Tasks;
using TickTrader.Algo.Core.Lib;

namespace TickTrader.Algo.Core
{
    public class LogFixture : CrossDomainObject, IPluginLogger
    {
        //private BufferBlock<PluginLogRecord> _logBuffer;
        //private ActionBlock<PluginLogRecord[]> _logSender;
        private IFixtureContext _context;
        private TimeKeyGenerator _keyGen = new TimeKeyGenerator();
        //private Task _batchLinkTask;
        private bool _isTradeBot;
        private string _indicatorPrefix;

        internal LogFixture(IFixtureContext context, bool isTradeBot)
        {
            _context = context;
            _isTradeBot = isTradeBot;
            if (!_isTradeBot)
                _indicatorPrefix = $"{context.InstanceId} ({context.MainSymbolCode}, {context.TimeFrame})";
        }

        public void Start()
        {
            _context.Builder.Logger = this;

            //if (!_context.IsGlobalUpdateMarshalingEnabled)
            //{
            //    var bufferOptions = new DataflowBlockOptions() { BoundedCapacity = 30 };
            //    var senderOptions = new ExecutionDataflowBlockOptions() { BoundedCapacity = 30 };

            //    _logBuffer = new BufferBlock<PluginLogRecord>(bufferOptions);
            //    _logSender = new ActionBlock<PluginLogRecord[]>(msgList =>
            //    {
            //        try
            //        {
            //            NewRecords?.Invoke(msgList);
            //        }
            //        catch (Exception ex)
            //        {
            //            _context.OnInternalException(ex);
            //        }
            //    }, senderOptions);

            //    _batchLinkTask = _logBuffer.BatchLinkTo(_logSender, 30);
            //}
        }

        public async Task Stop()
        {
            //if (!_context.IsGlobalUpdateMarshalingEnabled)
            //{
            //    try
            //    {
            //        _logBuffer.Complete();
            //        await _logBuffer.Completion;
            //        await _batchLinkTask;
            //        _logSender.Complete();
            //        await _logSender.Completion;
            //    }
            //    catch (Exception ex)
            //    {
            //        _context.OnInternalException(ex);
            //    }
            //}
        }

        //public event Action<PluginLogRecord[]> NewRecords;

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
                AddLogRecord(Domain.PluginLogRecord.Types.LogSeverity.Info, $"Bot started");
            else AddLogRecord(Domain.PluginLogRecord.Types.LogSeverity.Info, $"{_indicatorPrefix}: Indicator started");
        }

        public void OnStop()
        {
            if (_isTradeBot)
            {
                AddLogRecord(Domain.PluginLogRecord.Types.LogSeverity.Info, $"Bot stopped");
                AddLogRecord(Domain.PluginLogRecord.Types.LogSeverity.Info, $"Plugin version = {_context.Builder.Metadata.Descriptor.Version}");
            }
            else AddLogRecord(Domain.PluginLogRecord.Types.LogSeverity.Info, $"{_indicatorPrefix}: Indicator stopped");
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
                AddLogRecord(Domain.PluginLogRecord.Types.LogSeverity.CustomStatus, status);
        }

        private void AddLogRecord(Domain.PluginLogRecord.Types.LogSeverity logSeverity, string message, string errorDetails = null)
        {
            try
            {
                var timeKey = _keyGen.NextKey(DateTime.Now);
                var record = new Domain.PluginLogRecord(timeKey, logSeverity, message, errorDetails);
                //if (_logBuffer != null)
                //    _logBuffer.SendAsync(record).Wait();
                //else
                _context.SendNotification(record);
            }
            catch (Exception ex)
            {
                _context.OnInternalException(ex);
            }
        }
    }
}
