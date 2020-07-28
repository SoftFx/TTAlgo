using System;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.Core
{
    public static class ApiEnumConverter
    {
        public static CommissionType ToApiEnum(this Domain.CommissonInfo.Types.ValueType type)
        {
            switch (type)
            {
                case Domain.CommissonInfo.Types.ValueType.Points: return CommissionType.PerUnit;
                case Domain.CommissonInfo.Types.ValueType.Percentage: return CommissionType.Percent;
                case Domain.CommissonInfo.Types.ValueType.Money: return CommissionType.Absolute;

                default: throw new ArgumentException($"Unsupported commission charge type: {type}");
            }
        }

        public static AccountTypes ToApiEnum(this Domain.AccountInfo.Types.Type type)
        {
            switch (type)
            {
                case Domain.AccountInfo.Types.Type.Gross: return AccountTypes.Gross;
                case Domain.AccountInfo.Types.Type.Net: return AccountTypes.Net;
                case Domain.AccountInfo.Types.Type.Cash: return AccountTypes.Cash;

                default: throw new ArgumentException($"Unsupported commission charge type: {type}");
            }
        }

        public static OrderSide ToApiEnum(this Domain.OrderInfo.Types.Side side)
        {
            switch (side)
            {
                case Domain.OrderInfo.Types.Side.Buy: return OrderSide.Buy;
                case Domain.OrderInfo.Types.Side.Sell: return OrderSide.Sell;

                default: throw new ArgumentException($"Unsupported order side {side}");
            }
        }

        public static OrderType ToApiEnum(this Domain.OrderInfo.Types.Type type)
        {
            switch (type)
            {
                case Domain.OrderInfo.Types.Type.Limit: return OrderType.Limit;
                case Domain.OrderInfo.Types.Type.Market: return OrderType.Market;
                case Domain.OrderInfo.Types.Type.Stop: return OrderType.Stop;
                case Domain.OrderInfo.Types.Type.StopLimit: return OrderType.StopLimit;
                case Domain.OrderInfo.Types.Type.Position: return OrderType.Position;

                default: throw new ArgumentException($"Unsupported type {type}");
            }
        }

        public static SlippageType ToApiEnum(this Domain.SlippageInfo.Types.Type type)
        {
            switch (type)
            {
                case Domain.SlippageInfo.Types.Type.Percent: return SlippageType.Percent;
                case Domain.SlippageInfo.Types.Type.Pips: return SlippageType.Pips;

                default: throw new ArgumentException($"Unsupported type {type}");
            }
        }

        public static OrderOptions ToApiEnum(this Domain.OrderOptions options)
        {
            return (OrderOptions)options;
        }

        public static Domain.OrderExecOptions ToDomainEnum(this OrderExecOptions options)
        {
            return (Domain.OrderExecOptions)options;
        }

        public static Domain.OrderExecOptions ToDomainEnum(this OrderOptions options)
        {
            return (Domain.OrderExecOptions)options;
        }

        public static OrderCmdResultCodes ToApiEnum(this Domain.OrderExecReport.Types.CmdResultCode code)
        {
            switch (code)
            {
                case Domain.OrderExecReport.Types.CmdResultCode.Ok: return OrderCmdResultCodes.Ok;
                case Domain.OrderExecReport.Types.CmdResultCode.UnknownError: return OrderCmdResultCodes.UnknownError;
                case Domain.OrderExecReport.Types.CmdResultCode.InternalError: return OrderCmdResultCodes.InternalError;
                case Domain.OrderExecReport.Types.CmdResultCode.ConnectionError: return OrderCmdResultCodes.ConnectionError;
                case Domain.OrderExecReport.Types.CmdResultCode.Timeout: return OrderCmdResultCodes.Timeout;
                case Domain.OrderExecReport.Types.CmdResultCode.TradeServerError: return OrderCmdResultCodes.TradeServerError;
                case Domain.OrderExecReport.Types.CmdResultCode.DealerReject: return OrderCmdResultCodes.DealerReject;
                case Domain.OrderExecReport.Types.CmdResultCode.Unsupported: return OrderCmdResultCodes.Unsupported;
                case Domain.OrderExecReport.Types.CmdResultCode.SymbolNotFound: return OrderCmdResultCodes.SymbolNotFound;
                case Domain.OrderExecReport.Types.CmdResultCode.OrderNotFound: return OrderCmdResultCodes.OrderNotFound;
                case Domain.OrderExecReport.Types.CmdResultCode.IncorrectVolume: return OrderCmdResultCodes.IncorrectVolume;
                case Domain.OrderExecReport.Types.CmdResultCode.OffQuotes: return OrderCmdResultCodes.OffQuotes;
                case Domain.OrderExecReport.Types.CmdResultCode.IncorrectOrderId: return OrderCmdResultCodes.IncorrectOrderId;
                case Domain.OrderExecReport.Types.CmdResultCode.IncorrectPrice: return OrderCmdResultCodes.IncorrectPrice;
                case Domain.OrderExecReport.Types.CmdResultCode.IncorrectTp: return OrderCmdResultCodes.IncorrectTp;
                case Domain.OrderExecReport.Types.CmdResultCode.IncorrectSl: return OrderCmdResultCodes.IncorrectSl;
                case Domain.OrderExecReport.Types.CmdResultCode.NotEnoughMoney: return OrderCmdResultCodes.NotEnoughMoney;
                case Domain.OrderExecReport.Types.CmdResultCode.TradeNotAllowed: return OrderCmdResultCodes.TradeNotAllowed;
                case Domain.OrderExecReport.Types.CmdResultCode.IncorrectStopPrice: return OrderCmdResultCodes.IncorrectStopPrice;
                case Domain.OrderExecReport.Types.CmdResultCode.IncorrectMaxVisibleVolume: return OrderCmdResultCodes.IncorrectMaxVisibleVolume;
                case Domain.OrderExecReport.Types.CmdResultCode.IncorrectExpiration: return OrderCmdResultCodes.IncorrectExpiration;
                case Domain.OrderExecReport.Types.CmdResultCode.DealingTimeout: return OrderCmdResultCodes.DealingTimeout;
                case Domain.OrderExecReport.Types.CmdResultCode.Misconfiguration: return OrderCmdResultCodes.Misconfiguration;
                case Domain.OrderExecReport.Types.CmdResultCode.OrderLocked: return OrderCmdResultCodes.OrderLocked;
                case Domain.OrderExecReport.Types.CmdResultCode.IncorrectPricePrecision: return OrderCmdResultCodes.IncorrectPricePrecision;
                case Domain.OrderExecReport.Types.CmdResultCode.CloseOnlyTrading: return OrderCmdResultCodes.CloseOnlyTrading;
                case Domain.OrderExecReport.Types.CmdResultCode.MarketWithMaxVisibleVolume: return OrderCmdResultCodes.MarketWithMaxVisibleVolume;
                case Domain.OrderExecReport.Types.CmdResultCode.InvalidAmountChange: return OrderCmdResultCodes.InvalidAmountChange;
                case Domain.OrderExecReport.Types.CmdResultCode.CannotBeModified: return OrderCmdResultCodes.CannotBeModified;
                case Domain.OrderExecReport.Types.CmdResultCode.MaxVisibleVolumeNotSupported: return OrderCmdResultCodes.MaxVisibleVolumeNotSupported;
                case Domain.OrderExecReport.Types.CmdResultCode.ReadOnlyAccount: return OrderCmdResultCodes.ReadOnlyAccount;
                case Domain.OrderExecReport.Types.CmdResultCode.IncorrectSlippage: return OrderCmdResultCodes.IncorrectSlippage;

                default: throw new ArgumentException($"Unsupported code {code}");
            }

        }
    }
}