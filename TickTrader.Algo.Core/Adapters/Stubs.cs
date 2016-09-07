using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;
using TickTrader.Algo.Core.Lib;

namespace TickTrader.Algo.Core
{
    public interface IPluginLogger
    {
        void UpdateStatus(string status);
        void OnPrint(string entry, params object[] parameters);
        void OnPrintError(string entry, params object[] parameters);
        void OnError(Exception ex);
        void OnInitialized();
        void OnStart();
        void OnStop();
        void OnExit();
    }

    internal class PluginLoggerAdapter : IPluginMonitor
    {
        private IPluginLogger logger;

        public PluginLoggerAdapter(IPluginLogger logger)
        {
            this.logger = logger;
        }

        public void UpdateStatus(string status)
        {
            logger.UpdateStatus(status);
        }

        public void Print(string entry, object[] parameters)
        {
            logger.OnPrint(entry, parameters);
        }

        public void PrintError(string entry, object[] parameters)
        {
            logger.OnPrintError(entry, parameters);
        }
    }

    public class NullLogger : IPluginLogger
    {
        public void OnError(Exception ex)
        {
        }

        public void OnExit()
        {
        }

        public void OnInitialized()
        {
        }

        public void OnPrint(string entry, params object[] parameters)
        {
        }

        public void OnPrintError(string entry, params object[] parameters)
        {
        }

        public void OnStart()
        {
        }

        public void OnStop()
        {
        }

        public void UpdateStatus(string status)
        {
        }
    }

    public interface ITradeApi
    {
        void OpenOrder(TaskProxy<OrderCmdResult> waitHandler, string symbol, OrderType type, OrderSide side, double price, double volume, double? tp, double? sl, string comment);
        void CancelOrder(TaskProxy<OrderCmdResult> waitHandler, string orderId, OrderSide side);
        void ModifyOrder(TaskProxy<OrderCmdResult> waitHandler, string orderId, string symbol, OrderType type, OrderSide side, double price, double volume, double? tp, double? sl, string comment);
        void CloseOrder(TaskProxy<OrderCmdResult> waitHandler, string orderId, double? volume);
    }

    internal class NullTradeApi : TradeCommands
    {
        private static Task<OrderCmdResult> rejectResult
            = Task.FromResult<OrderCmdResult>(new TradeResultEntity(OrderCmdResultCodes.Unsupported, OrderEntity.Null));

        public Task<OrderCmdResult> CancelOrder(string orderId)
        {
            return rejectResult;
        }

        public Task<OrderCmdResult> CloseOrder(string orderId, double? volume)
        {
            return rejectResult;
        }

        public Task<OrderCmdResult> ModifyOrder(string orderId, double price, double? tp, double? sl, string comment)
        {
            return rejectResult;
        }

        public Task<OrderCmdResult> OpenOrder(string symbol, OrderType type, OrderSide side, double price, double volume, double? tp, double? sl, string comment)
        {
            return rejectResult;
        }
    }
}
