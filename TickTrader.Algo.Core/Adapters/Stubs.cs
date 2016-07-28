using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.Core
{
    public interface IPluginLogger
    {
        void UpdateStatus(string status);
        void WriteLog(string entry, params object[] parameters);
        void WriteError(string msg, string description);
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

        public void WriteLog(string entry, object[] parameters)
        {
            logger.WriteLog(entry, parameters);
        }
    }

    public class NullLogger : Api.IPluginMonitor
    {
        public void UpdateStatus(string status)
        {
        }

        public void WriteLog(string entry, object[] parameters)
        {
        }
    }

    public interface ITradeApi
    {
        OrderCmdResult OpenOrder(OpenOrdeRequest request);
        OrderCmdResult CancelOrder(CancelOrdeRequest request);
        OrderCmdResult ModifyOrder(ModifyOrdeRequest request);
        OrderCmdResult CloseOrder(CloseOrdeRequest request);
    }

    internal class TradeApiAdapter : Api.ITradeCommands
    {
        private ITradeApi api;

        public TradeApiAdapter(ITradeApi api)
        {
            this.api = api;
        }

        public OrderCmdResult CancelOrder(CancelOrdeRequest request)
        {
            return api.CancelOrder(request);
        }

        public OrderCmdResult CloseOrder(CloseOrdeRequest request)
        {
            return api.CloseOrder(request);
        }

        public OrderCmdResult ModifyOrder(ModifyOrdeRequest request)
        {
            return api.ModifyOrder(request);
        }

        public OrderCmdResult OpenOrder(OpenOrdeRequest request)
        {
            return api.OpenOrder(request);
        }
    }

    internal class NullTradeApi : Api.ITradeCommands
    {
        private static TradeResultEntity rejectResult = new TradeResultEntity(OrderCmdResultCodes.Unsupported, OrderEntity.Null);

        public OrderCmdResult CancelOrder(CancelOrdeRequest request)
        {
            return rejectResult;
        }

        public OrderCmdResult CloseOrder(CloseOrdeRequest request)
        {
            return rejectResult;
        }

        public OrderCmdResult ModifyOrder(ModifyOrdeRequest request)
        {
            return rejectResult;
        }

        public OrderCmdResult OpenOrder(OpenOrdeRequest request)
        {
            return rejectResult;
        }
    }
}
