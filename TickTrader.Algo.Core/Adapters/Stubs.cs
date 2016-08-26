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
        void OnError(Exception ex);
        void OnInitialized();
        void OnStart();
        void OnStop();
        void OnExit();
    }

    internal class PluginLoggerAdapter : Api.IPluginMonitor
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
        void OpenOrder(OpenOrdeRequest request, TaskProxy<OrderCmdResult> waitHandler);
        void CancelOrder(CancelOrdeRequest request, TaskProxy<OrderCmdResult> waitHandler);
        void ModifyOrder(ModifyOrdeRequest request, TaskProxy<OrderCmdResult> waitHandler);
        void CloseOrder(CloseOrdeRequest request, TaskProxy<OrderCmdResult> waitHandler);
    }

    internal class NullTradeApi : ITradeCommands
    {
        private static Task<OrderCmdResult> rejectResult
            = Task.FromResult<OrderCmdResult>(new TradeResultEntity(OrderCmdResultCodes.Unsupported, OrderEntity.Null));

        public Task<OrderCmdResult> OpenMarketOrder(string symbolCode, OrderSides side, OrderVolume volume, double? stopLoss = default(double?), double? takeProfit = default(double?), string comment = null)
        {
            return rejectResult;
        }

        public Task<OrderCmdResult> CancelOrder(CancelOrdeRequest request)
        {
            return rejectResult;
        }

        public Task<OrderCmdResult> CloseOrder(CloseOrdeRequest request)
        {
            return rejectResult;
        }

        public Task<OrderCmdResult> ModifyOrder(ModifyOrdeRequest request)
        {
            return rejectResult;
        }

        public Task<OrderCmdResult> OpenOrder(OpenOrdeRequest request)
        {
            return rejectResult;
        }

        public Task<OrderCmdResult> CloseOrder(string orderId, double? closeVolume)
        {
            return rejectResult;
        }
    }
}
