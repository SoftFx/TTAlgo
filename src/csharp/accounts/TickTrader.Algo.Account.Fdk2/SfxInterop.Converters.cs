using Google.Protobuf.WellKnownTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using TickTrader.Algo.Domain;
using TickTrader.FDK.Common;
using SFX = TickTrader.FDK.Common;

namespace TickTrader.Algo.Account.Fdk2
{
    public partial class SfxInterop
    {
        private static Domain.SymbolInfo Convert(SFX.SymbolInfo info)
        {
            return new Domain.SymbolInfo
            {
                Name = info.Name,
                TradeAllowed = info.IsTradeEnabled,
                BaseCurrency = info.Currency,
                CounterCurrency = info.SettlementCurrency,
                Digits = info.Precision,
                LotSize = info.RoundLot,
                MinTradeVolume = info.RoundLot != 0 ? info.MinTradeVolume / info.RoundLot : double.NaN,
                MaxTradeVolume = info.RoundLot != 0 ? info.MaxTradeVolume / info.RoundLot : double.NaN,
                TradeVolumeStep = info.RoundLot != 0 ? info.TradeVolumeStep / info.RoundLot : double.NaN,

                Description = info.Description,
                Security = info.SecurityName,
                GroupSortOrder = info.GroupSortOrder,
                SortOrder = info.SortOrder,

                Slippage = new Domain.SlippageInfo
                {
                    DefaultValue = info.DefaultSlippage,
                    Type = Convert(info.SlippageType),
                },

                Commission = new Domain.CommissonInfo
                {
                    Commission = info.Commission,
                    LimitsCommission = info.LimitsCommission,
                    ValueType = Convert(info.CommissionType),
                    MinCommission = info.MinCommission,
                    MinCommissionCurrency = info.MinCommissionCurrency,
                },

                Margin = new Domain.MarginInfo
                {
                    Mode = Convert(info.MarginCalcMode),
                    Factor = info.MarginFactorFractional ?? 1,
                    Hedged = info.MarginHedge,
                    StopOrderReduction = info.StopOrderMarginReduction,
                    HiddenLimitOrderReduction = info.HiddenLimitOrderMarginReduction,
                },

                Swap = new Domain.SwapInfo
                {
                    Enabled = info.SwapEnabled,
                    Type = Convert(info.SwapType),
                    SizeLong = info.SwapSizeLong,
                    SizeShort = info.SwapSizeShort,
                    TripleSwapDay = info.TripleSwapDay,
                },
            };
        }

        private static Domain.SlippageInfo.Types.Type Convert(SFX.SlippageType type)
        {
            switch (type)
            {
                case SFX.SlippageType.Pips: return Domain.SlippageInfo.Types.Type.Pips;
                case SFX.SlippageType.Percent: return Domain.SlippageInfo.Types.Type.Percent;
                default: throw new NotImplementedException();
            }
        }

        private static Domain.SwapInfo.Types.Type Convert(SwapType type)
        {
            switch (type)
            {
                case SwapType.PercentPerYear: return Domain.SwapInfo.Types.Type.PercentPerYear;
                case SwapType.Points: return Domain.SwapInfo.Types.Type.Points;
                default: throw new NotImplementedException();
            }
        }

        private static Domain.CommissonInfo.Types.ValueType Convert(SFX.CommissionType fdkType)
        {
            switch (fdkType)
            {
                case SFX.CommissionType.Absolute: return Domain.CommissonInfo.Types.ValueType.Money;
                case SFX.CommissionType.PerUnit: return Domain.CommissonInfo.Types.ValueType.Points;
                case SFX.CommissionType.Percent: return Domain.CommissonInfo.Types.ValueType.Percentage;

                // Server is not using those anymore. Providing fallback value just in case
                case SFX.CommissionType.PerBond:
                case SFX.CommissionType.PercentageWaivedCash:
                case SFX.CommissionType.PercentageWaivedEnhanced:
                    return Domain.CommissonInfo.Types.ValueType.Percentage;

                default: throw new ArgumentException("Unsupported commission type: " + fdkType);
            }
        }

        private static Domain.MarginInfo.Types.CalculationMode Convert(MarginCalcMode mode)
        {
            switch (mode)
            {
                case MarginCalcMode.Cfd: return Domain.MarginInfo.Types.CalculationMode.Cfd;
                case MarginCalcMode.CfdIndex: return Domain.MarginInfo.Types.CalculationMode.CfdIndex;
                case MarginCalcMode.CfdLeverage: return Domain.MarginInfo.Types.CalculationMode.CfdLeverage;
                case MarginCalcMode.Forex: return Domain.MarginInfo.Types.CalculationMode.Forex;
                case MarginCalcMode.Futures: return Domain.MarginInfo.Types.CalculationMode.Futures;
                default: throw new NotImplementedException();
            }
        }


        private static Domain.CurrencyInfo Convert(SFX.CurrencyInfo info)
        {
            return new Domain.CurrencyInfo()
            {
                Name = info.Name,
                Digits = info.Precision,
                SortOrder = info.SortOrder,
                Type = info.TypeId,
            };
        }

        private static Domain.AccountInfo.Types.Type Convert(AccountType fdkType)
        {
            switch (fdkType)
            {
                case AccountType.Cash: return Domain.AccountInfo.Types.Type.Cash;
                case AccountType.Gross: return Domain.AccountInfo.Types.Type.Gross;
                case AccountType.Net: return Domain.AccountInfo.Types.Type.Net;

                default: throw new ArgumentException("Unsupported account type: " + fdkType);
            }
        }

        private static Domain.OrderInfo.Types.Type Convert(SFX.OrderType fdkType)
        {
            switch (fdkType)
            {
                case SFX.OrderType.Limit: return Domain.OrderInfo.Types.Type.Limit;
                case SFX.OrderType.Market: return Domain.OrderInfo.Types.Type.Market;
                case SFX.OrderType.Position: return Domain.OrderInfo.Types.Type.Position;
                case SFX.OrderType.Stop: return Domain.OrderInfo.Types.Type.Stop;
                case SFX.OrderType.StopLimit: return Domain.OrderInfo.Types.Type.StopLimit;

                default: throw new ArgumentException("Unsupported order type: " + fdkType);
            }
        }

        private static SFX.OrderType Convert(Domain.OrderInfo.Types.Type type)
        {
            switch (type)
            {
                case Domain.OrderInfo.Types.Type.Market: return SFX.OrderType.Market;
                case Domain.OrderInfo.Types.Type.Position: return SFX.OrderType.Position;
                case Domain.OrderInfo.Types.Type.StopLimit: return SFX.OrderType.StopLimit;
                case Domain.OrderInfo.Types.Type.Limit: return SFX.OrderType.Limit;
                case Domain.OrderInfo.Types.Type.Stop: return SFX.OrderType.Stop;

                default: throw new ArgumentException("Unsupported order type: " + type);
            }
        }

        private static Domain.OrderInfo.Types.Side Convert(SFX.OrderSide fdkSide)
        {
            switch (fdkSide)
            {
                case SFX.OrderSide.Buy: return Domain.OrderInfo.Types.Side.Buy;
                case SFX.OrderSide.Sell: return Domain.OrderInfo.Types.Side.Sell;

                default: throw new ArgumentException("Unsupported order side: " + fdkSide);
            }
        }

        private static SFX.OrderSide Convert(Domain.OrderInfo.Types.Side side)
        {
            switch (side)
            {
                case Domain.OrderInfo.Types.Side.Buy: return SFX.OrderSide.Buy;
                case Domain.OrderInfo.Types.Side.Sell: return SFX.OrderSide.Sell;

                default: throw new ArgumentException("Unsupported order side: " + side);
            }
        }

        private static BarPeriod ToBarPeriod(Domain.Feed.Types.Timeframe timeframe)
        {
            switch (timeframe)
            {
                case Domain.Feed.Types.Timeframe.MN: return BarPeriod.MN1;
                case Domain.Feed.Types.Timeframe.W: return BarPeriod.W1;
                case Domain.Feed.Types.Timeframe.D: return BarPeriod.D1;
                case Domain.Feed.Types.Timeframe.H4: return BarPeriod.H4;
                case Domain.Feed.Types.Timeframe.H1: return BarPeriod.H1;
                case Domain.Feed.Types.Timeframe.M30: return BarPeriod.M30;
                case Domain.Feed.Types.Timeframe.M15: return BarPeriod.M15;
                case Domain.Feed.Types.Timeframe.M5: return BarPeriod.M5;
                case Domain.Feed.Types.Timeframe.M1: return BarPeriod.M1;
                case Domain.Feed.Types.Timeframe.S10: return BarPeriod.S10;
                case Domain.Feed.Types.Timeframe.S1: return BarPeriod.S1;

                default: throw new ArgumentException("Unsupported time frame: " + timeframe);
            }
        }

        private static Domain.AccountInfo Convert(SFX.AccountInfo info)
        {
            return new Domain.AccountInfo(info.Type != AccountType.Cash ? info.Balance : null, info.Currency, info.Assets.Select(Convert))
            {
                Id = info.AccountId,
                Type = Convert(info.Type),
                Leverage = info.Leverage ?? 1,
            };
        }

        private static Domain.AssetInfo Convert(SFX.AssetInfo info)
        {
            return new Domain.AssetInfo
            {
                Currency = info.Currency,
                Balance = info.Balance,
            };
        }

        public static Domain.OrderInfo Convert(SFX.ExecutionReport record)
        {
            var userTag = record.Tag;
            var instanceId = "";

            if (Domain.CompositeTag.TryParse(record.Tag, out var tag))
            {
                instanceId = tag?.Key;
                userTag = tag?.Tag;
            }

            return new Domain.OrderInfo
            {
                Id = record.OrderId,
                Symbol = record.Symbol,
                Comment = record.Comment,
                InitialType = Convert(record.InitialOrderType),
                Type = Convert(record.OrderType),
                Price = record.Price,
                StopPrice = record.StopPrice,
                Side = Convert(record.OrderSide),
                Created = record.Created?.ToTimestamp(),
                Swap = record.Swap,
                Modified = record.Modified?.ToTimestamp(),
                StopLoss = record.StopLoss,
                TakeProfit = record.TakeProfit,
                Slippage = record.Slippage,
                Commission = record.Commission,
                ExecAmount = record.ExecutedVolume,
                UserTag = userTag,
                InstanceId = instanceId,
                RemainingAmount = record.LeavesVolume,
                RequestedAmount = record.InitialVolume ?? 0,
                Expiration = record.Expiration.ToUtcTicks(),
                MaxVisibleAmount = record.MaxVisibleVolume,
                ExecPrice = record.AveragePrice,
                RequestedOpenPrice = record.InitialPrice,
                Options = GetOptions(record),
                LastFillPrice = record.TradePrice,
                LastFillAmount = record.TradeAmount,
                ParentOrderId = record.ParentOrderId,
                OcoRelatedOrderId = record.RelatedOrderId?.ToString(),
                OtoTrigger = GetOtoTrigger(record),
            };
        }

        private static Domain.OrderOptions GetOptions(SFX.ExecutionReport record)
        {
            var result = Domain.OrderOptions.None;
            var isLimit = record.OrderType == SFX.OrderType.Limit || record.OrderType == SFX.OrderType.StopLimit;

            if (isLimit && record.ImmediateOrCancelFlag)
                result |= Domain.OrderOptions.ImmediateOrCancel;

            if (record.MarketWithSlippage)
                result |= Domain.OrderOptions.MarketWithSlippage;

            if (record.MaxVisibleVolume.HasValue)
                result |= Domain.OrderOptions.HiddenIceberg;

            if (record.OneCancelsTheOtherFlag)
                result |= Domain.OrderOptions.OneCancelsTheOther;

            if (record.ContingentOrderFlag)
                result |= Domain.OrderOptions.ContingentOrder;

            return result;
        }

        private static Domain.ContingentOrderTrigger GetOtoTrigger(SFX.ExecutionReport report)
        {
            if (!report.ContingentOrderFlag)
                return null;

            return new Domain.ContingentOrderTrigger
            {
                Type = ConvertToAlgo(report.TriggerType.Value),
                TriggerTime = report.TriggerTime.ToUtcTicks(),
                OrderIdTriggeredBy = report.OrderIdTriggeredBy?.ToString(),
            };
        }

        private static List<ExecutionReport> ConvertToEr(List<SFX.ExecutionReport> reports)
        {
            var result = new List<ExecutionReport>(reports.Count);

            for (int i = 0; i < reports.Count; i++)
                result.Add(ConvertToEr(reports[i], reports[i].OrigClientOrderId));

            return result;
        }

        private static ExecutionReport ConvertToEr(SFX.ExecutionReport report, string operationId = null)
        {
            if (report.ExecutionType == SFX.ExecutionType.Rejected && report.RejectReason == RejectReason.None)
                report.RejectReason = RejectReason.Other; // Some plumbing. Sometimes we recieve Rejects with no RejectReason

            return new ExecutionReport()
            {
                Id = report.OrderId,
                ParentOrderId = report.ParentOrderId,
                // ExecTime = report.???
                TradeRequestId = operationId,
                Expiration = report.Expiration,
                Created = report.Created,
                Modified = report.Modified,
                RejectReason = Convert(report.RejectReason, report.Text ?? ""),
                TakeProfit = report.TakeProfit,
                StopLoss = report.StopLoss,
                Slippage = report.Slippage,
                Text = report.Text,
                Comment = report.Comment,
                UserTag = report.Tag,
                Magic = report.Magic,
                IsReducedOpenCommission = report.ReducedOpenCommission,
                IsReducedCloseCommission = report.ReducedCloseCommission,
                ImmediateOrCancel = report.ImmediateOrCancelFlag,
                MarketWithSlippage = report.MarketWithSlippage,
                TradePrice = report.TradePrice ?? 0,
                RequestedOpenPrice = report.InitialPrice,
                Assets = report.Assets.Select(Convert).ToArray(),
                StopPrice = report.StopPrice,
                AveragePrice = report.AveragePrice,
                ClientOrderId = report.ClientOrderId,
                OrigClientOrderId = report.OrigClientOrderId,
                OrderStatus = Convert(report.OrderStatus),
                ExecutionType = Convert(report.ExecutionType),
                Symbol = report.Symbol,
                ExecutedVolume = report.ExecutedVolume,
                InitialVolume = report.InitialVolume,
                LeavesVolume = report.LeavesVolume,
                MaxVisibleAmount = report.MaxVisibleVolume,
                TradeAmount = report.TradeAmount,
                Commission = report.Commission,
                AgentCommission = report.AgentCommission,
                Swap = report.Swap,
                InitialType = Convert(report.InitialOrderType),
                Type = Convert(report.OrderType),
                Side = Convert(report.OrderSide),
                Price = report.Price,
                Balance = report.Balance ?? double.NaN,
                IsOneCancelsTheOther = report.OneCancelsTheOtherFlag,
                OcoRelatedOrderId = report.RelatedOrderId?.ToString(),
                IsContingentOrder = report.ContingentOrderFlag,
                OtoTrigger = GetOtoTrigger(report),
            };
        }

        private static ExecutionType Convert(SFX.ExecutionType type)
        {
            return (ExecutionType)type;
        }

        private static OrderStatus Convert(SFX.OrderStatus status)
        {
            return (OrderStatus)status;
        }

        private static ContingentOrderTriggerType? ConvertToServer(Domain.ContingentOrderTrigger.Types.TriggerType? type)
        {
            switch (type)
            {
                case ContingentOrderTrigger.Types.TriggerType.OnPendingOrderExpired:
                    return ContingentOrderTriggerType.OnPendingOrderExpired;
                case ContingentOrderTrigger.Types.TriggerType.OnPendingOrderPartiallyFilled:
                    return ContingentOrderTriggerType.OnPendingOrderPartiallyFilled;
                case ContingentOrderTrigger.Types.TriggerType.OnTime:
                    return ContingentOrderTriggerType.OnTime;

                default:
                    return null;
            }
        }

        private static Domain.ContingentOrderTrigger.Types.TriggerType ConvertToAlgo(ContingentOrderTriggerType type)
        {
            return (Domain.ContingentOrderTrigger.Types.TriggerType)type;
        }

        private static Domain.OrderExecReport.Types.CmdResultCode Convert(RejectReason reason, string message)
        {
            switch (reason)
            {
                case RejectReason.InternalServerError: return Domain.OrderExecReport.Types.CmdResultCode.TradeServerError;
                case RejectReason.DealerReject: return Domain.OrderExecReport.Types.CmdResultCode.DealerReject;
                case RejectReason.UnknownSymbol: return Domain.OrderExecReport.Types.CmdResultCode.SymbolNotFound;
                case RejectReason.UnknownOrder: return Domain.OrderExecReport.Types.CmdResultCode.OrderNotFound;
                case RejectReason.IncorrectQuantity: return Domain.OrderExecReport.Types.CmdResultCode.IncorrectVolume;
                case RejectReason.OffQuotes: return Domain.OrderExecReport.Types.CmdResultCode.OffQuotes;
                case RejectReason.OrderExceedsLimit: return Domain.OrderExecReport.Types.CmdResultCode.NotEnoughMoney; // open market order with low acc balance
                case RejectReason.OrdersLimitExceeded: return Domain.OrderExecReport.Types.CmdResultCode.ExceededOrderLimit;
                case RejectReason.CloseOnly: return Domain.OrderExecReport.Types.CmdResultCode.CloseOnlyTrading;
                case RejectReason.ThrottlingLimits: return Domain.OrderExecReport.Types.CmdResultCode.ThrottlingError;
                case RejectReason.Other:
                    {
                        if (message != null)
                        {
                            if (message == "Trade Not Allowed" || message == "Trade is not allowed!")
                                return Domain.OrderExecReport.Types.CmdResultCode.TradeNotAllowed;
                            else if (message.StartsWith("Not Enough Money"))
                                return Domain.OrderExecReport.Types.CmdResultCode.NotEnoughMoney;
                            else if (message.StartsWith("Rejected By Dealer"))
                                return Domain.OrderExecReport.Types.CmdResultCode.DealerReject;
                            else if (message.StartsWith("Dealer") && message.EndsWith("did not respond."))
                                return Domain.OrderExecReport.Types.CmdResultCode.DealingTimeout;
                            else if (message.Contains("locked by another operation"))
                                return Domain.OrderExecReport.Types.CmdResultCode.OrderLocked;
                            else if (message.Contains("Invalid expiration"))
                                return Domain.OrderExecReport.Types.CmdResultCode.IncorrectExpiration;
                            else if (message.StartsWith("Price precision"))
                                return Domain.OrderExecReport.Types.CmdResultCode.IncorrectPricePrecision;
                            else if (message.EndsWith("because close-only mode on"))
                                return Domain.OrderExecReport.Types.CmdResultCode.CloseOnlyTrading;
                            else if (message.StartsWith("Max visible amount is not valid for") || message.StartsWith("Max visible amount is valid only for"))
                                return Domain.OrderExecReport.Types.CmdResultCode.MaxVisibleVolumeNotSupported;
                            else if (message.StartsWith("Order Not Found") || message.EndsWith("was not found."))
                                return Domain.OrderExecReport.Types.CmdResultCode.OrderNotFound;
                            else if (message.StartsWith("Invalid order type") || message.Contains("is not supported") ||
                                    (message.StartsWith("Option") && message.Contains("cannot be used")) ||
                                    message.StartsWith("OCO flag is used only for"))
                                return Domain.OrderExecReport.Types.CmdResultCode.Unsupported;
                            else if (message.StartsWith("Invalid AmountChange") || message == "Cannot modify amount.")
                                return Domain.OrderExecReport.Types.CmdResultCode.InvalidAmountChange;
                            else if (message == "Account Is Readonly")
                                return Domain.OrderExecReport.Types.CmdResultCode.ReadOnlyAccount;
                            else if (message == "Internal server error")
                                return Domain.OrderExecReport.Types.CmdResultCode.TradeServerError;
                            else if (message.StartsWith("Only Limit, Stop and StopLimit orders can be canceled."))
                                return Domain.OrderExecReport.Types.CmdResultCode.IncorrectType;
                            else if (message.Contains("was called too frequently"))
                                return Domain.OrderExecReport.Types.CmdResultCode.ThrottlingError;
                            else if (message.StartsWith("OCO flag requires to specify"))
                                return Domain.OrderExecReport.Types.CmdResultCode.OcoRelatedIdNotFound;
                            else if (message.EndsWith("already has OCO flag!"))
                                return Domain.OrderExecReport.Types.CmdResultCode.OcoRelatedOrderAlreadyExists;
                            else if (message.StartsWith("Buy price must be less than"))
                                return Domain.OrderExecReport.Types.CmdResultCode.IncorrectPrice;
                            else if (message.Contains("has different Symbol"))
                                return Domain.OrderExecReport.Types.CmdResultCode.IncorrectSymbol;
                            else if (message.EndsWith("Remove OCO relation first.") ||
                                    (message.StartsWith("Related order") && message.EndsWith("already has OCO flag.")))
                                return Domain.OrderExecReport.Types.CmdResultCode.OcoAlreadyExists;
                            else if (message.StartsWith("No Dealer"))
                                return Domain.OrderExecReport.Types.CmdResultCode.DealerReject;
                            else if (message.StartsWith("Trigger time cannot") ||
                                     message.Contains("must have the same trigger time"))
                                return Domain.OrderExecReport.Types.CmdResultCode.IncorrectTriggerTime;
                            else if (message.StartsWith("Trigger Order Id"))
                                return Domain.OrderExecReport.Types.CmdResultCode.IncorrectTriggerOrderId;
                            else if ((message.Contains("Trigger Order") && message.Contains("Type must be")) ||
                                      message.StartsWith("Contingent orders with type") && message.EndsWith("are not supported.") ||
                                      message.Contains("must have the same trigger type"))
                                return Domain.OrderExecReport.Types.CmdResultCode.IncorrectTriggerOrderType;
                            else if (message.Contains("Trigger Order") && message.Contains("Expired"))
                                return Domain.OrderExecReport.Types.CmdResultCode.IncorrectExpiration;
                            else if (message.EndsWith("cannot be Contingent order"))
                                return Domain.OrderExecReport.Types.CmdResultCode.IncorrectConditionsForTrigger;
                            else if (message.StartsWith("Cannot create OCO relation between"))
                                return Domain.OrderExecReport.Types.CmdResultCode.OcoRelatedOrderIncorrectOptions;
                            else if (message.StartsWith("OCO order cannot be related to itself"))
                                return Domain.OrderExecReport.Types.CmdResultCode.OcoIncorrectRelatedId;
                            else if (message == "Positions have the same side.")
                                return Domain.OrderExecReport.Types.CmdResultCode.IncorrectSide;
                        }
                        break;
                    }
                case RejectReason.None:
                    {
                        if (message != null && (message.StartsWith("Order Not Found") || message.EndsWith("not found.")))
                            return Domain.OrderExecReport.Types.CmdResultCode.OrderNotFound;
                        return Domain.OrderExecReport.Types.CmdResultCode.Ok;
                    }
            }
            return Domain.OrderExecReport.Types.CmdResultCode.UnknownError;
        }

        private static Domain.PositionInfo Convert(SFX.Position p)
        {
            Domain.OrderInfo.Types.Side side;
            double price;
            double amount;

            if (p.BuyAmount > 0)
            {
                side = Domain.OrderInfo.Types.Side.Buy;
                price = p.BuyPrice ?? 0;
                amount = p.BuyAmount;
            }
            else
            {
                side = Domain.OrderInfo.Types.Side.Sell;
                price = p.SellPrice ?? 0;
                amount = p.SellAmount;
            }

            return new Domain.PositionInfo
            {
                Id = p.PosId,
                Symbol = p.Symbol,
                Side = side,
                Volume = amount,
                Price = price,
                Commission = p.Commission,
                Swap = p.Swap,
                Modified = p.Modified?.ToTimestamp(),
            };
        }

        private static Domain.PositionExecReport ConvertToReport(SFX.Position p)
        {
            return new Domain.PositionExecReport
            {
                PositionCopy = Convert(p),
                ExecAction = Convert(p.PosReportType),
            };
        }

        private static Domain.OrderExecReport.Types.ExecAction Convert(SFX.PosReportType type)
        {
            switch (type)
            {
                case SFX.PosReportType.CancelPosition:
                    return Domain.OrderExecReport.Types.ExecAction.Canceled;

                case SFX.PosReportType.ModifyPosition:
                    return Domain.OrderExecReport.Types.ExecAction.Modified;

                case SFX.PosReportType.ClosePosition:
                    return Domain.OrderExecReport.Types.ExecAction.Closed;

                case SFX.PosReportType.Split:
                    return Domain.OrderExecReport.Types.ExecAction.Splitted;

                default:
                    return Domain.OrderExecReport.Types.ExecAction.None;
            }
        }


        internal static Domain.BarData Convert(SFX.Bar fdkBar)
        {
            return new Domain.BarData(new UtcTicks(fdkBar.From), new UtcTicks(fdkBar.To))
            {
                Open = fdkBar.Open,
                Close = fdkBar.Close,
                High = fdkBar.High,
                Low = fdkBar.Low,
                RealVolume = fdkBar.Volume,
            };
        }

        internal static Domain.QuoteInfo[] Convert(SFX.Quote[] fdkQuoteSnapshot)
        {
            var result = new Domain.QuoteInfo[fdkQuoteSnapshot.Length];

            for (var i = 0; i < result.Length; i++)
                result[i] = Convert(fdkQuoteSnapshot[i]);
            return result;
        }

        private static Domain.QuoteInfo Convert(SFX.Quote fdkTick)
        {
            var timeOfReceive = DateTime.UtcNow;

            var time = new UtcTicks(fdkTick.CreatingTime);
            var bids = ConvertLevel2(fdkTick.Bids);
            var asks = ConvertLevel2(fdkTick.Asks);
            return new QuoteInfo(fdkTick.Symbol, time, bids, asks, timeOfReceive: timeOfReceive)
            {
                IsBidIndicative = fdkTick.TickType == SFX.TickTypes.IndicativeBid || fdkTick.TickType == SFX.TickTypes.IndicativeBidAsk,
                IsAskIndicative = fdkTick.TickType == SFX.TickTypes.IndicativeAsk || fdkTick.TickType == SFX.TickTypes.IndicativeBidAsk,
            };
        }

        private static byte[] ConvertLevel2(List<QuoteEntry> book)
        {
            var cnt = book.Count;
            var bytes = new byte[QuoteBand.Size * cnt];
            var bands = MemoryMarshal.Cast<byte, QuoteBand>(bytes);

            for (var i = 0; i < cnt; i++)
            {
                bands[i] = new QuoteBand(book[i].Price, book[i].Volume);
            }

            return bytes;
        }

        public static Domain.TradeReportInfo Convert(TradeTransactionReport report)
        {
            bool isBalanceTransaction = report.TradeTransactionReportType == TradeTransactionReportType.Credit
                || report.TradeTransactionReportType == TradeTransactionReportType.BalanceTransaction;

            var userTag = report.Tag;
            var instanceId = "";

            if (Domain.CompositeTag.TryParse(report.Tag, out var tag))
            {
                instanceId = tag?.Key;
                userTag = tag?.Tag;
            }

            return new Domain.TradeReportInfo()
            {
                IsEmulated = false,
                Id = report.Id,
                OrderId = report.Id,
                TransactionReason = Convert(report.TradeTransactionReason),
                ReportType = Convert(report.TradeTransactionReportType),
                Symbol = isBalanceTransaction ? report.TransactionCurrency : report.Symbol,
                TakeProfit = report.TakeProfit,
                StopLoss = report.StopLoss,
                Comment = report.Comment,
                Commission = report.Commission,
                CommissionCurrency = report.CommCurrency ?? report.DstAssetCurrency ?? report.TransactionCurrency,
                OpenQuantity = report.Quantity,
                PositionCloseQuantity = report.PositionLastQuantity,
                PositionClosePrice = report.PositionClosePrice,
                Swap = report.Swap,
                RemainingQuantity = report.LeavesQuantity,
                AccountBalance = report.AccountBalance,
                ActionId = report.ActionId,
                DstAssetAmount = report.DstAssetAmount,
                DstAssetCurrency = report.DstAssetCurrency,
                DstAssetMovement = report.DstAssetMovement,
                Expiration = report.Expiration?.ToUniversalTime().ToTimestamp(),
                OrderOptions = GetOptions(report),
                MarginCurrency = report.MarginCurrency,
                MaxVisibleQuantity = report.MaxVisibleQuantity,
                OrderOpened = report.OrderCreated.ToUniversalTime().ToTimestamp(),
                OrderFillPrice = report.OrderFillPrice,
                OrderLastFillAmount = report.OrderLastFillAmount,
                OrderModified = report.OrderModified.ToUniversalTime().ToTimestamp(),
                PositionById = report.PositionById,
                PositionClosed = report.PositionClosed.ToUniversalTime().ToTimestamp(),
                PositionId = report.PositionId,
                PositionLeavesQuantity = report.PositionLeavesQuantity,
                PositionModified = report.PositionModified.ToUniversalTime().ToTimestamp(),
                PositionOpened = report.PositionOpened.ToUniversalTime().ToTimestamp(),
                PositionQuantity = report.PositionQuantity,
                PositionOpenPrice = report.PosOpenPrice,
                PositionRemainingPrice = report.PosRemainingPrice,
                PositionRemainingSide = Convert(report.PosRemainingSide),
                Price = report.Price,
                ProfitCurrency = report.ProfitCurrency,
                RequestedClosePrice = report.ReqClosePrice,
                RequestedCloseQuantity = report.ReqCloseQuantity,
                RequestedOpenPrice = report.ReqOpenPrice,
                SrcAssetAmount = report.SrcAssetAmount,
                SrcAssetCurrency = report.SrcAssetCurrency,
                SrcAssetMovement = report.SrcAssetMovement,
                OrderSide = Convert(report.OrderSide),
                OrderType = Convert(report.OrderType),
                RequestedOpenQuantity = report.ReqOpenQuantity,
                StopPrice = report.StopPrice,
                Tag = userTag,
                InstanceId = instanceId,
                TransactionAmount = report.TransactionAmount,
                TransactionCurrency = report.TransactionCurrency,
                TransactionTime = report.TransactionTime.ToUniversalTime().ToTimestamp(),
                RequestedOrderType = Convert(report.ReqOrderType != null ? report.ReqOrderType.Value : report.OrderType),
                SplitRatio = report.SplitRatio,
                Tax = report.Tax,
                Slippage = report.Slippage,
                OcoRelatedOrderId = report.RelatedOrderId?.ToString(),
            };
        }

        private static Domain.TradeReportInfo.Types.ReportType Convert(TradeTransactionReportType type)
        {
            switch (type)
            {
                case TradeTransactionReportType.BalanceTransaction: return Domain.TradeReportInfo.Types.ReportType.BalanceTransaction;
                case TradeTransactionReportType.Credit: return Domain.TradeReportInfo.Types.ReportType.Credit;
                case TradeTransactionReportType.OrderActivated: return Domain.TradeReportInfo.Types.ReportType.OrderActivated;
                case TradeTransactionReportType.OrderCanceled: return Domain.TradeReportInfo.Types.ReportType.OrderCanceled;
                case TradeTransactionReportType.OrderExpired: return Domain.TradeReportInfo.Types.ReportType.OrderExpired;
                case TradeTransactionReportType.OrderFilled: return Domain.TradeReportInfo.Types.ReportType.OrderFilled;
                case TradeTransactionReportType.OrderOpened: return Domain.TradeReportInfo.Types.ReportType.OrderOpened;
                case TradeTransactionReportType.PositionClosed: return Domain.TradeReportInfo.Types.ReportType.PositionClosed;
                case TradeTransactionReportType.PositionOpened: return Domain.TradeReportInfo.Types.ReportType.PositionOpened;
                case TradeTransactionReportType.TradeModified: return Domain.TradeReportInfo.Types.ReportType.TradeModified;
                default: return Domain.TradeReportInfo.Types.ReportType.NoType;
            }
        }

        private static Domain.TradeReportInfo.Types.Reason Convert(SFX.TradeTransactionReason reason)
        {
            switch (reason)
            {
                case SFX.TradeTransactionReason.ClientRequest: return Domain.TradeReportInfo.Types.Reason.ClientRequest;
                case SFX.TradeTransactionReason.PendingOrderActivation: return Domain.TradeReportInfo.Types.Reason.PendingOrderActivation;
                case SFX.TradeTransactionReason.StopOut: return Domain.TradeReportInfo.Types.Reason.StopOut;
                case SFX.TradeTransactionReason.StopLossActivation: return Domain.TradeReportInfo.Types.Reason.StopLossActivation;
                case SFX.TradeTransactionReason.TakeProfitActivation: return Domain.TradeReportInfo.Types.Reason.TakeProfitActivation;
                case SFX.TradeTransactionReason.DealerDecision: return Domain.TradeReportInfo.Types.Reason.DealerDecision;
                case SFX.TradeTransactionReason.Rollover: return Domain.TradeReportInfo.Types.Reason.Rollover;
                case SFX.TradeTransactionReason.DeleteAccount: return Domain.TradeReportInfo.Types.Reason.DeleteAccount;
                case SFX.TradeTransactionReason.Expired: return Domain.TradeReportInfo.Types.Reason.Expired;
                case SFX.TradeTransactionReason.TransferMoney: return Domain.TradeReportInfo.Types.Reason.TransferMoney;
                case SFX.TradeTransactionReason.Split: return Domain.TradeReportInfo.Types.Reason.Split;
                case SFX.TradeTransactionReason.Dividend: return Domain.TradeReportInfo.Types.Reason.Dividend;
                case SFX.TradeTransactionReason.OneCancelsTheOther: return Domain.TradeReportInfo.Types.Reason.OneCancelsTheOther;
                default: return Domain.TradeReportInfo.Types.Reason.NoReason;
            }
        }

        private static Domain.OrderOptions GetOptions(TradeTransactionReport report)
        {
            var res = Domain.OrderOptions.None;

            if (report.ImmediateOrCancel)
                res |= Domain.OrderOptions.ImmediateOrCancel;
            if (report.MarketWithSlippage)
                res |= Domain.OrderOptions.MarketWithSlippage;
            if (report.MaxVisibleQuantity.HasValue)
                res |= Domain.OrderOptions.HiddenIceberg;
            if (report.RelatedOrderId != null)
                res |= Domain.OrderOptions.OneCancelsTheOther;

            return res;
        }

        public static Domain.TriggerReportInfo Convert(SFX.ContingentOrderTriggerReport report)
        {
            return new Domain.TriggerReportInfo
            {
                Id = report.Id,
                ContingentOrderId = report.ContingentOrderId.ToString(),
                TransactionTime = report.TransactionTime.ToTimestamp(),
                TriggerType = ConvertToAlgo(report.TriggerType),
                TriggerState = Convert(report.TriggerState),
                TriggerTime = report.TriggerTime?.ToTimestamp(),
                OrderIdTriggeredBy = report.OrderIdTriggeredBy?.ToString(),
                Symbol = report.Symbol,
                Type = Convert(report.Type),
                Side = Convert(report.Side),
                Price = report.Price,
                StopPrice = report.StopPrice,
                Amount = report.Amount,
                RelatedOrderId = report.RelatedOrderId?.ToString(),
            };
        }

        public static Domain.TriggerReportInfo.Types.TriggerResultState Convert(TriggerResultState state)
        {
            return (Domain.TriggerReportInfo.Types.TriggerResultState)state;
        }

        public static Domain.BalanceOperation Convert(SFX.BalanceOperation op)
        {
            return new Domain.BalanceOperation
            {
                Balance = op.Balance,
                Currency = op.TransactionCurrency,
                TransactionAmount = op.TransactionAmount,
                Type = Convert(op.TransactionType),
            };
        }

        public static Domain.BalanceOperation.Types.Type Convert(SFX.BalanceTransactionType type)
        {
            switch (type)
            {
                case BalanceTransactionType.DepositWithdrawal:
                    return Domain.BalanceOperation.Types.Type.DepositWithdrawal;
                case BalanceTransactionType.Dividend:
                    return Domain.BalanceOperation.Types.Type.Dividend;
            }

            throw new NotImplementedException("Unsupported balance transaction type: " + type);
        }

        internal static PriceType ConvertBack(Domain.Feed.Types.MarketSide marketSide)
        {
            switch (marketSide)
            {
                case Domain.Feed.Types.MarketSide.Ask: return PriceType.Ask;
                case Domain.Feed.Types.MarketSide.Bid: return PriceType.Bid;
            }
            throw new NotImplementedException("Unsupported market side: " + marketSide);
        }

        private static ConnectionErrorInfo.Types.ErrorCode Convert(LogoutReason fdkCode)
        {
            switch (fdkCode)
            {
                case LogoutReason.BlockedAccount: return ConnectionErrorInfo.Types.ErrorCode.BlockedAccount;
                case LogoutReason.InvalidCredentials: return ConnectionErrorInfo.Types.ErrorCode.InvalidCredentials;
                case LogoutReason.NetworkError: return ConnectionErrorInfo.Types.ErrorCode.NetworkError;
                case LogoutReason.ServerError: return ConnectionErrorInfo.Types.ErrorCode.ServerError;
                case LogoutReason.ServerLogout: return ConnectionErrorInfo.Types.ErrorCode.ServerLogout;
                case LogoutReason.SlowConnection: return ConnectionErrorInfo.Types.ErrorCode.SlowConnection;
                case LogoutReason.LoginDeleted: return ConnectionErrorInfo.Types.ErrorCode.LoginDeleted;
                default: return ConnectionErrorInfo.Types.ErrorCode.UnknownConnectionError;
            }
        }

        internal static BarUpdateSummary[] Convert(AggregatedBarUpdate[] fdkUpdates)
        {
            var res = new BarUpdateSummary[fdkUpdates.Length];
            for (var i = 0; i < fdkUpdates.Length; i++)
            {
                res[i] = Convert(fdkUpdates[i]);
                res[i].IsReset = true;
            }
            return res;
        }

        internal static BarUpdateSummary Convert(AggregatedBarUpdate fdkUpdate)
        {
            var res = new BarUpdateSummary
            {
                Symbol = fdkUpdate.Symbol,
                AskClose = fdkUpdate.AskClose,
                BidClose = fdkUpdate.BidClose,
                AskVolumeDelta = fdkUpdate.AskVolumeDelta,
                BidVolumeDelta = fdkUpdate.BidVolumeDelta,
                CloseOnly = fdkUpdate.IsClosedBarUpdate,
            };
            if (fdkUpdate.Updates?.Count > 0)
            {
                res.Details = fdkUpdate.Updates.Select(Convert).ToArray();
            }
            return res;
        }

        internal static BarUpdateDetails Convert(KeyValuePair<BarParameters, SFX.BarUpdate> fdkUpdate)
        {
            var k = fdkUpdate.Key;
            var v = fdkUpdate.Value;
            return new BarUpdateDetails
            {
                Timeframe = Convert(k.Periodicity),
                MarketSide = Convert(k.PriceType),
                From = v.From,
                Open = v.Open,
                High = v.High,
                Low = v.Low,
                Volume = v.Volume,
            };
        }

        internal static Feed.Types.MarketSide Convert(PriceType priceType)
        {
            switch (priceType)
            {
                case PriceType.Ask: return Feed.Types.MarketSide.Ask;
                case PriceType.Bid: return Feed.Types.MarketSide.Bid;
            }
            throw new NotImplementedException("Unsupported price type: " + priceType);
        }

        internal static Feed.Types.Timeframe Convert(Periodicity periodicity)
        {
            switch (periodicity.Interval)
            {
                case SFX.Time.TimeInterval.Second:
                    switch (periodicity.IntervalsCount)
                    {
                        case 1: return Feed.Types.Timeframe.S1;
                        case 10: return Feed.Types.Timeframe.S10;
                    }
                    break;
                case SFX.Time.TimeInterval.Minute:
                    switch (periodicity.IntervalsCount)
                    {
                        case 1: return Feed.Types.Timeframe.M1;
                        case 5: return Feed.Types.Timeframe.M5;
                        case 15: return Feed.Types.Timeframe.M15;
                        case 30: return Feed.Types.Timeframe.M30;
                    }
                    break;
                case SFX.Time.TimeInterval.Hour:
                    switch (periodicity.IntervalsCount)
                    {
                        case 1: return Feed.Types.Timeframe.H1;
                        case 4: return Feed.Types.Timeframe.H4;
                    }
                    break;
                case SFX.Time.TimeInterval.Day:
                    switch (periodicity.IntervalsCount)
                    {
                        case 1: return Feed.Types.Timeframe.D;
                    }
                    break;
                case SFX.Time.TimeInterval.Week:
                    switch (periodicity.IntervalsCount)
                    {
                        case 1: return Feed.Types.Timeframe.W;
                    }
                    break;
                case SFX.Time.TimeInterval.Month:
                    switch (periodicity.IntervalsCount)
                    {
                        case 1: return Feed.Types.Timeframe.MN;
                    }
                    break;
            }
            throw new NotImplementedException("Unsupported periodicity: " + periodicity);
        }

        internal static Periodicity ConvertBack(Feed.Types.Timeframe timeframe)
        {
            switch (timeframe)
            {
                case Feed.Types.Timeframe.S1: return new Periodicity(SFX.Time.TimeInterval.Second, 1);
                case Feed.Types.Timeframe.S10: return new Periodicity(SFX.Time.TimeInterval.Second, 10);
                case Feed.Types.Timeframe.M1: return new Periodicity(SFX.Time.TimeInterval.Minute, 1);
                case Feed.Types.Timeframe.M5: return new Periodicity(SFX.Time.TimeInterval.Minute, 5);
                case Feed.Types.Timeframe.M15: return new Periodicity(SFX.Time.TimeInterval.Minute, 15);
                case Feed.Types.Timeframe.M30: return new Periodicity(SFX.Time.TimeInterval.Minute, 30);
                case Feed.Types.Timeframe.H1: return new Periodicity(SFX.Time.TimeInterval.Hour, 1);
                case Feed.Types.Timeframe.H4: return new Periodicity(SFX.Time.TimeInterval.Hour, 4);
                case Feed.Types.Timeframe.D: return new Periodicity(SFX.Time.TimeInterval.Day, 1);
                case Feed.Types.Timeframe.W: return new Periodicity(SFX.Time.TimeInterval.Week, 1);
                case Feed.Types.Timeframe.MN: return new Periodicity(SFX.Time.TimeInterval.Month, 1);
            }
            throw new NotImplementedException("Unsupported timeframe: " + timeframe);
        }
    }
}
