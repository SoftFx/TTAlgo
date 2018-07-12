using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Core.Lib;

namespace TickTrader.Algo.Core
{
    /// <summary>
    /// Collects:
    ///     1. Events (user logs + orders)
    ///     2. Outputs
    /// </summary>
    internal class BacktesterCollector : CrossDomainObject, IPluginLogger
    {
        private InvokeEmulator _invokeEmulator;
        private PluginExecutor _executor;
        private List<BotLogRecord> _events = new List<BotLogRecord>();
        private Dictionary<string, object> _outputBuffers = new Dictionary<string, object>();

        public long TicksCount { get; set; }
        public int OrdersOpened { get; set; }
        public int OrdersRejected { get; set; }
        public int Modifications { get; set; }
        public int ModificationRejected { get; set; }

        public BacktesterCollector(PluginExecutor executor, InvokeEmulator emulator)
        {
            _executor = executor;
            _invokeEmulator = emulator;
            _invokeEmulator.RateUpdated += r => TicksCount++;
        }

        private DateTime VirtualTimepoint => _invokeEmulator.VirtualTimePoint;

        public int EventsCount => _events.Count;

        public void AddEvent(LogSeverities severity, string message, string description = null)
        {
            _events.Add(new BotLogRecord(VirtualTimepoint, severity, message, description));
        }

        public void LogTrade(string message)
        {
            AddEvent(LogSeverities.TradeSuccess, message);
        }

        public void LogTradeFail(string message)
        {
            AddEvent(LogSeverities.TradeFail, message);
        }

        public IPagedEnumerator<BotLogRecord> GetEvents()
        {
            return _events.GetCrossDomainEnumerator(1000);
        }

        public void InitOutputCollection<T>(string id)
        {
            var output = _executor.GetOutput<T>(id);
            var outputBuffer = new List<T>();

            output.Appended += a =>
            {
                outputBuffer.Add(a.Value);
            };
        }

        public List<T> GetOutputBuffer<T>(string id)
        {
            return (List<T>)_outputBuffers[id];
        }

        #region IPluginLogger

        void IPluginLogger.OnAbort()
        {
        }

        void IPluginLogger.OnError(Exception ex)
        {
            AddEvent(LogSeverities.Error, ex.Message);
        }

        void IPluginLogger.OnError(string message, Exception ex)
        {
            AddEvent(LogSeverities.Error, message);
        }

        void IPluginLogger.OnExit()
        {
        }

        void IPluginLogger.OnInitialized()
        {
            AddEvent(LogSeverities.Info, "Initialized");
        }

        void IPluginLogger.OnPrint(string entry)
        {
            AddEvent(LogSeverities.Custom, entry);
        }

        void IPluginLogger.OnPrint(string entry, params object[] parameters)
        {
            AddEvent(LogSeverities.Custom, string.Format(entry, parameters));
        }

        void IPluginLogger.OnPrintError(string entry)
        {
            AddEvent(LogSeverities.Error, entry);
        }

        void IPluginLogger.OnPrintError(string entry, params object[] parameters)
        {
            AddEvent(LogSeverities.Error, string.Format(entry, parameters));
        }

        void IPluginLogger.OnPrintInfo(string info)
        {
        }

        void IPluginLogger.OnPrintTrade(string entry)
        {
        }

        void IPluginLogger.OnPrintTradeFail(string entry)
        {
        }

        void IPluginLogger.OnPrintTradeSuccess(string entry)
        {
        }

        void IPluginLogger.OnStart()
        {
            AddEvent(LogSeverities.Info, "Start");
        }

        void IPluginLogger.OnStop()
        {
            AddEvent(LogSeverities.Info, "Stop");
        }

        void IPluginLogger.UpdateStatus(string status)
        {
        }

        #endregion
    }
}
