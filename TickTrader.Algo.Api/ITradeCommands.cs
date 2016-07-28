using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Api
{
    internal interface ITradeCommands
    {
        OrderCmdResult OpenOrder(OpenOrdeRequest request);
        OrderCmdResult CancelOrder(CancelOrdeRequest request);
        OrderCmdResult ModifyOrder(ModifyOrdeRequest request);
        OrderCmdResult CloseOrder(CloseOrdeRequest request);
    }

    [Serializable]
    public class OpenOrdeRequest
    {
        public OrderTypes Type { get; set; }
        public OrderSides Side { get; set; }
        public string SymbolCode { get; set; }
        public OrderVolume Volume { get; set; }
        public double Price { get; set; }
        public double? TaskProfit { get; set; }
        public double? StopLoss { get; set; }
        public string Comment { get; set; }
    }

    [Serializable]
    public class ModifyOrdeRequest
    {
        public int OrderId { get; set; }
        public OrderVolume Volume { get; set; }
        public double Price { get; set; }
        public double? TaskProfit { get; set; }
        public double? StopLoss { get; set; }
        public string Comment { get; set; }
    }

    [Serializable]
    public class CloseOrdeRequest
    {
        public int OrderId { get; set; }
        public OrderVolume Volume { get; set; }
    }

    [Serializable]
    public class CancelOrdeRequest
    {
        public int OrderId { get; set; }
    }

    public interface OrderCmdResult
    {
        OrderCmdResultCodes ResultCode { get; }
        bool IsCompleted { get; }
        bool IsFaulted { get; }
        Order ResultingOrder { get; }
    }

    public enum VolumeUnits { Lots, CurrencyUnits }

    public struct OrderVolume
    {
        public OrderVolume(double value, VolumeUnits units)
        {
            Value = value;
            Units = units;
        }

        public static OrderVolume Lots(double value)
        {
            return new OrderVolume(value, VolumeUnits.Lots);
        }

        public static OrderVolume Absolute(double value)
        {
            return new OrderVolume(value, VolumeUnits.CurrencyUnits);
        }

        public VolumeUnits Units { get; private set; }
        public double Value { get; private set; }
    }

    public enum OrderCmdResultCodes
    {
        Ok,
        DealerReject,
        ConnectionError,
        Unsupported
    }
}
