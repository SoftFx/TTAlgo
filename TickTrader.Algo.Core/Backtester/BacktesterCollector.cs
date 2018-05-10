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
        private List<BotLogRecord> _events = new List<BotLogRecord>();

        public BacktesterCollector(InvokeEmulator emulator)
        {
            _invokeEmulator = emulator;
        }

        private DateTime VirtualTimepoint => _invokeEmulator.VirtualTimePoint;

        public int EventsCount => _events.Count;

        public void AddEvent(LogSeverities severity, string message, string description = null)
        {
            _events.Add(new BotLogRecord(VirtualTimepoint, severity, message, description));
        }

        public void LogTrade(string description)
        {
        }

        public IPagedEnumerator<BotLogRecord> GetEvents()
        {
            return _events.GetCrossDomainEnumerator(1000);
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
