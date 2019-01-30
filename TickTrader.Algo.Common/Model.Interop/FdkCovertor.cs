using SoftFX.Extended;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Api = TickTrader.Algo.Api;
using TickTrader.Algo.Core;
using BO = TickTrader.BusinessObjects;
using SoftFX.Extended.Reports;

namespace TickTrader.Algo.Common.Model
{
    public static class FdkConvertor
    {
        public static IEnumerable<BarEntity> ConvertAndFilter(this IEnumerable<Bar> srcBars)
        {
            return ConvertAndFilter(srcBars, DateTime.MinValue);
        }

        public static IEnumerable<BarEntity> ConvertAndFilter(this IEnumerable<Bar> srcBars, DateTime initialTimeEdge)
        {
            var timeEdge = initialTimeEdge;

            foreach (var bar in srcBars)
            {
                if (bar.From > timeEdge)
                {
                    yield return Convert(bar);
                    timeEdge = bar.From;
                }
            }
        }

        public static void Convert(IEnumerable<Bar> srcBars, List<BarEntity> dstBars)
        {
            foreach (var bar in srcBars)
                dstBars.Add(Convert(bar));
        }

        public static List<BarEntity> Convert(Bar[] srcBars, bool invert)
        {
            var result = new List<BarEntity>(srcBars.Length);

            if (invert)
            {
                for (int i = srcBars.Length - 1; i >= 0; i--)
                    result.Add(Convert(srcBars[i]));
            }
            else
            {
                for (int i = 0; i < srcBars.Length; i++)
                    result.Add(Convert(srcBars[i]));
            }

            return result;
        }

        public static IEnumerable<BarEntity> Convert(IEnumerable<Bar> fdkBarCollection)
        {
            return fdkBarCollection.Select(Convert);
        }

        public static IEnumerable<QuoteEntity> Convert(IEnumerable<Quote> fdkQuoteArray)
        {
            return fdkQuoteArray.Select(Convert);
        }

        public static AccountEntity Convert(AccountInfo info)
        {
            return new AccountEntity()
            {
                Id = info.AccountId,
                Balance = info.Balance,
                BalanceCurrency = info.Currency,
                Type = Convert(info.Type),
                Leverage = info.Leverage,
                Assets = info.Assets.Select(Convert).ToArray()
            };
        }

        public static AssetEntity Convert(AssetInfo info)
        {
            return new AssetEntity(info.Balance, info.Currency)
            {
                TradeVolume = info.TradeAmount
            };
        }

        public static OrderEntity Convert(TradeRecord record)
        {
            return new OrderEntity(record.OrderId)
            {
                Symbol = record.Symbol,
                Comment = record.Comment,
                InitialType = Convert(record.InitialType) ?? Api.OrderType.Market,
                Type = Convert(record.Type) ?? Api.OrderType.Market,
                ClientOrderId = record.ClientOrderId,
                Price = record.Price,
                StopPrice = record.StopPrice,
                Side = Convert(record.Side).Value,
                Created = record.Created,
                Swap = record.Swap,
                Modified = record.Modified,
                StopLoss = record.StopLoss,
                TakeProfit = record.TakeProfit,
                Commission = record.Commission,
                UserTag = record.Tag,
                RequestedVolume = record.InitialVolume,
                RemainingVolume = record.Volume,
                MaxVisibleVolume = record.MaxVisibleVolume,
                Expiration = record.Expiration,
            };
        }

        public static PositionEntity Convert(Position p)
        {
            Api.OrderSide side;
            double price;
            double amount;

            if (p.BuyAmount > 0)
            {
                side = Api.OrderSide.Buy;
                price = p.BuyPrice ?? 0;
                amount = p.BuyAmount;
            }
            else
            {
                side = Api.OrderSide.Sell;
                price = p.SellPrice ?? 0;
                amount = p.SellAmount;
            }

            return new PositionEntity()
            {
                Side = side,
                Volume = amount,
                Price = price,
                Symbol = p.Symbol,
                Commission = p.Commission,
                AgentCommission = p.AgentCommission,
                Swap = p.Swap
            };
        }

        public static BarEntity Convert(Bar fdkBar)
        {
            return new BarEntity()
            {
                Open = fdkBar.Open,
                Close = fdkBar.Close,
                High = fdkBar.High,
                Low = fdkBar.Low,
                Volume = fdkBar.Volume,
                OpenTime = fdkBar.From,
                CloseTime = fdkBar.To
            };
        }

        public static QuoteEntity Convert(Quote fdkTick)
        {
            return new QuoteEntity(fdkTick.Symbol, fdkTick.CreatingTime, ConvertLevel2(fdkTick.Bids), ConvertLevel2(fdkTick.Asks));
        }

        private static Api.BookEntry[] ConvertLevel2(QuoteEntry[] book)
        {
            if (book == null || book.Length == 0)
                return QuoteEntity.EmptyBook;
            else
                return book.Select(b => Convert(b)).ToArray();
        }

        public static BookEntryEntity Convert(QuoteEntry fdkEntry)
        {
            return new BookEntryEntity()
            {
                Price = fdkEntry.Price,
                Volume = fdkEntry.Volume
            };
        }

        public static SymbolEntity[] Convert(SymbolInfo[] info)
        {
            return info.Select(Convert).ToArray();
        }

        public static SymbolEntity Convert(SymbolInfo info)
        {
            return new SymbolEntity(info.Name)
            {
                Digits = info.Precision,
                LotSize = info.RoundLot,
                MinAmount = info.RoundLot != 0 ? info.MinTradeVolume / info.RoundLot : double.NaN,
                MaxAmount = info.RoundLot != 0 ? info.MaxTradeVolume / info.RoundLot : double.NaN,
                AmountStep = info.RoundLot != 0 ? info.TradeVolumeStep / info.RoundLot : double.NaN,
                BaseCurrencyCode = info.Currency,
                CounterCurrencyCode = info.SettlementCurrency,
                IsTradeAllowed = info.IsTradeEnabled,
                Commission = info.Commission,
                LimitsCommission = info.LimitsCommission,
                CommissionChargeMethod = Convert(info.CommissionChargeMethod),
                CommissionChargeType = Convert(info.CommissionChargeType),
                CommissionType = Convert(info.CommissionType),
                ContractSizeFractional = info.RoundLot,
                MarginFactorFractional = info.MarginFactorFractional ?? 1,
                StopOrderMarginReduction = info.StopOrderMarginReduction ?? 0,
                MarginHedged = info.MarginHedge,
                MarginMode = Convert(info.MarginCalcMode),
                SwapEnabled = true, // ???
                SwapSizeLong = (float)(info.SwapSizeLong ?? 0),
                SwapSizeShort = (float)(info.SwapSizeShort ?? 0),
                Security = info.SecurityName,
                GroupSortOrder = info.GroupSortOrder,
                SortOrder = info.SortOrder,
                SwapType = Convert(info.SwapType),
                TripleSwapDay = info.TripleSwapDay,
                IsTradeEnabled = info.IsTradeEnabled,
                Description = info.Description,
                DefaultSlippage = info.DefaultSlippage,
                HiddenLimitOrderMarginReduction = info.HiddenLimitOrderMarginReduction
            };
        }

        public static BO.SwapType Convert(SwapType type)
        {
            switch (type)
            {
                case SwapType.PercentPerYear: return BO.SwapType.PercentPerYear;
                case SwapType.Points: return BO.SwapType.Points;
                default: throw new NotImplementedException();
            }
        }

        public static TickTrader.BusinessObjects.MarginCalculationModes Convert(MarginCalcMode mode)
        {
            switch (mode)
            {
                case MarginCalcMode.Cfd: return BusinessObjects.MarginCalculationModes.CFD;
                case MarginCalcMode.CfdIndex: return BusinessObjects.MarginCalculationModes.CFD_Index;
                case MarginCalcMode.CfdLeverage: return BusinessObjects.MarginCalculationModes.CFD_Leverage;
                case MarginCalcMode.Forex: return BusinessObjects.MarginCalculationModes.Forex;
                case MarginCalcMode.Futures: return BusinessObjects.MarginCalculationModes.Futures;
                default: throw new NotImplementedException();
            }
        }

        public static CurrencyEntity[] Convert(CurrencyInfo[] info)
        {
            return info.Select(Convert).ToArray();
        }

        public static CurrencyEntity Convert(CurrencyInfo info)
        {
            return new CurrencyEntity(info.Name, info.Precision)
            {
                SortOrder = info.SortOrder
            };
        }

        public static Api.AccountTypes Convert(AccountType fdkType)
        {
            switch (fdkType)
            {
                case AccountType.Cash: return Algo.Api.AccountTypes.Cash;
                case AccountType.Gross: return Algo.Api.AccountTypes.Gross;
                case AccountType.Net: return Algo.Api.AccountTypes.Net;

                default: throw new ArgumentException("Unsupported account type: " + fdkType);
            }
        }

        public static Api.OrderType? Convert(TradeRecordType fdkType)
        {
            if (fdkType == (TradeRecordType)(-1)) // ugly fix for FDK bug
                return null;

            switch (fdkType)
            {
                case TradeRecordType.Limit: return Algo.Api.OrderType.Limit;
                case TradeRecordType.Market: return Algo.Api.OrderType.Market;
                case TradeRecordType.Position: return Algo.Api.OrderType.Position;
                case TradeRecordType.Stop: return Algo.Api.OrderType.Stop;
                case TradeRecordType.StopLimit: return Algo.Api.OrderType.StopLimit;
                case TradeRecordType.StopLimit_IoC: return Algo.Api.OrderType.StopLimit;

                default: throw new ArgumentException("Unsupported order type: " + fdkType);
            }
        }

        public static Api.OrderSide? Convert(TradeRecordSide fdkSide)
        {
            if (fdkSide == (TradeRecordSide)(-1)) // ugly fix for FDK bug
                return null;

            switch (fdkSide)
            {
                case TradeRecordSide.Buy: return Api.OrderSide.Buy;
                case TradeRecordSide.Sell: return Api.OrderSide.Sell;

                default: throw new ArgumentException("Unsupported order side: " + fdkSide);
            }
        }

        public static BarPeriod ToBarPeriod(Api.TimeFrames timeframe)
        {
            switch (timeframe)
            {
                case Api.TimeFrames.MN: return BarPeriod.MN1;
                case Api.TimeFrames.W: return BarPeriod.W1;
                case Api.TimeFrames.D: return BarPeriod.D1;
                case Api.TimeFrames.H4: return BarPeriod.H4;
                case Api.TimeFrames.H1: return BarPeriod.H1;
                case Api.TimeFrames.M30: return BarPeriod.M30;
                case Api.TimeFrames.M15: return BarPeriod.M15;
                case Api.TimeFrames.M5: return BarPeriod.M5;
                case Api.TimeFrames.M1: return BarPeriod.M1;
                case Api.TimeFrames.S10: return BarPeriod.S10;
                case Api.TimeFrames.S1: return BarPeriod.S1;

                default: throw new ArgumentException("Unsupported time frame: " + timeframe);
            }
        }

        public static Api.CommissionType Convert(CommissionType fdkType)
        {
            switch (fdkType)
            {
                case CommissionType.Absolute: return Api.CommissionType.Absolute;
                case CommissionType.PerBond: return Api.CommissionType.PerBond;
                case CommissionType.PerUnit: return Api.CommissionType.PerUnit;
                case CommissionType.Percent: return Api.CommissionType.Percent;
                case CommissionType.PercentageWaivedCash: return Api.CommissionType.PercentageWaivedCash;
                case CommissionType.PercentageWaivedEnhanced: return Api.CommissionType.PercentageWaivedEnhanced;

                default: throw new ArgumentException("Unsupported commission type: " + fdkType);
            }
        }

        public static Api.CommissionChargeType Convert(CommissionChargeType fdkChargeType)
        {
            switch (fdkChargeType)
            {
                case CommissionChargeType.PerLot: return Api.CommissionChargeType.PerLot;
                case CommissionChargeType.PerTrade: return Api.CommissionChargeType.PerTrade;

                default: throw new ArgumentException("Unsupported commission charge type: " + fdkChargeType);
            }
        }

        public static Api.CommissionChargeMethod Convert(CommissionChargeMethod fdkChargeMethod)
        {
            switch (fdkChargeMethod)
            {
                case CommissionChargeMethod.OneWay: return Api.CommissionChargeMethod.OneWay;
                case CommissionChargeMethod.RoundTurn: return Api.CommissionChargeMethod.RoundTurn;

                default: throw new ArgumentException("Unsupported commission charge method: " + fdkChargeMethod);
            }
        }

        public static Api.OrderCmdResultCodes Convert(RejectReason reason, string message)
        {
            switch (reason)
            {
                case RejectReason.DealerReject: return Api.OrderCmdResultCodes.DealerReject;
                case RejectReason.UnknownSymbol: return Api.OrderCmdResultCodes.SymbolNotFound;
                case RejectReason.UnknownOrder: return Api.OrderCmdResultCodes.OrderNotFound;
                case RejectReason.IncorrectQuantity: return Api.OrderCmdResultCodes.IncorrectVolume;
                case RejectReason.OffQuotes: return Api.OrderCmdResultCodes.OffQuotes;
                case RejectReason.OrderExceedsLImit: return Api.OrderCmdResultCodes.NotEnoughMoney;
                case RejectReason.Other:
                    {
                        if (message == "Trade Not Allowed")
                            return Api.OrderCmdResultCodes.TradeNotAllowed;
                        else if (message != null && message.StartsWith("Dealer") && message.EndsWith("did not respond."))
                            return Api.OrderCmdResultCodes.DealingTimeout;
                        else if (message != null && message.Contains("locked by another operation"))
                            return Api.OrderCmdResultCodes.OrderLocked;
                        break;
                    }
                case RejectReason.None:
                    {
                        if (message.StartsWith("Order Not Found"))
                            return Api.OrderCmdResultCodes.OrderNotFound;
                        else if (message.StartsWith("Max Visible Amount"))
                            return Api.OrderCmdResultCodes.IncorrectMaxVisibleVolume;
                        else if (message.StartsWith("Invalid Amount"))
                            return Api.OrderCmdResultCodes.IncorrectVolume;
                        break;
                    }
            }
            return Api.OrderCmdResultCodes.UnknownError;
        }

        public static Api.OrderCmdResultCodes Convert(ArgumentOutOfRangeException ex)
        {
            var paramName = ex.ParamName.ToLowerInvariant();
            if (paramName.Contains("maxvisiblevolume"))
            {
                return Api.OrderCmdResultCodes.IncorrectMaxVisibleVolume;
            }
            else if (paramName.Contains("volume"))
            {
                return Api.OrderCmdResultCodes.IncorrectVolume;
            }
            else if (paramName.Contains("stopprice"))
            {
                return Api.OrderCmdResultCodes.IncorrectStopPrice;
            }
            else if (paramName.Contains("price"))
            {
                return Api.OrderCmdResultCodes.IncorrectPrice;
            }
            else if (paramName.Contains("stoploss"))
            {
                return Api.OrderCmdResultCodes.IncorrectSl;
            }
            else if (paramName.Contains("takeprofit"))
            {
                return Api.OrderCmdResultCodes.IncorrectTp;
            }
            return Api.OrderCmdResultCodes.InternalError;
        }

        public static TradeReportEntity Convert(TradeTransactionReport report)
        {
            bool isBalanceTransaction = report.TradeTransactionReportType == TradeTransactionReportType.Credit
                || report.TradeTransactionReportType == TradeTransactionReportType.BalanceTransaction;

            return new TradeReportEntity()
            {
                Id = report.Id,
                OrderId = report.Id,
                OpenTime = report.OrderCreated,
                CloseTime = report.TransactionTime,
                Type = GetRecordType(report),
                ActionType = Convert(report.TradeTransactionReportType),
                Symbol = isBalanceTransaction ? report.TransactionCurrency : report.Symbol,
                TakeProfit = report.TakeProfit,
                StopLoss = report.StopLoss,
                OpenPrice = report.Price,
                Comment = report.Comment,
                Commission = report.Commission,
                CommissionCurrency = report.DstAssetCurrency ?? report.TransactionCurrency,
                OpenQuantity = report.Quantity,
                CloseQuantity = report.PositionLastQuantity,
                TransactionAmount = report.TransactionAmount,
                ClosePrice = report.PositionClosePrice,
                Swap = report.Swap,
                RemainingQuantity = report.LeavesQuantity,
                AccountBalance = report.AccountBalance,
                ActionId = report.ActionId,
                AgentCommission = report.AgentCommission,
                ClientId = report.ClientId,
                CloseConversionRate = report.CloseConversionRate,
                CommCurrency = report.CommCurrency,
                DstAssetAmount = report.DstAssetAmount,
                DstAssetCurrency = report.DstAssetCurrency,
                DstAssetMovement = report.DstAssetMovement,
                DstAssetToUsdConversionRate = report.DstAssetToUsdConversionRate,
                Expiration = report.Expiration,
                ImmediateOrCancel = report.ImmediateOrCancel,
                IsReducedCloseCommission = report.IsReducedCloseCommission,
                IsReducedOpenCommission = report.IsReducedOpenCommission,
                LeavesQuantity = report.LeavesQuantity,
                Magic = report.Magic,
                MarginCurrency = report.MarginCurrency,
                MarginCurrencyToUsdConversionRate = report.MarginCurrencyToUsdConversionRate,
                MarketWithSlippage = report.MarketWithSlippage,
                MaxVisibleQuantity = report.MaxVisibleQuantity,
                MinCommissionConversionRate = report.MinCommissionConversionRate,
                MinCommissionCurrency = report.MinCommissionCurrency,
                NextStreamPositionId = report.NextStreamPositionId,
                OpenConversionRate = report.OpenConversionRate,
                OrderCreated = report.OrderCreated,
                OrderFillPrice = report.OrderFillPrice,
                OrderLastFillAmount = report.OrderLastFillAmount,
                OrderModified = report.OrderModified,
                PositionById = report.PositionById,
                PositionClosed = report.PositionClosed,
                PositionClosePrice = report.PositionClosePrice,
                PositionCloseRequestedPrice = report.PositionCloseRequestedPrice,
                PositionId = report.PositionId,
                PositionLastQuantity = report.PositionLastQuantity,
                PositionLeavesQuantity = report.PositionLeavesQuantity,
                PositionModified = report.PositionModified,
                PositionOpened = report.PositionOpened,
                PositionQuantity = report.PositionQuantity,
                PosOpenPrice = report.PosOpenPrice,
                PosOpenReqPrice = report.PosOpenReqPrice,
                PosRemainingPrice = report.PosRemainingPrice,
                PosRemainingSide = Convert(report.PosRemainingSide),
                Price = report.Price,
                ProfitCurrency = report.ProfitCurrency,
                ProfitCurrencyToUsdConversionRate = report.ProfitCurrencyToUsdConversionRate,
                ReqClosePrice = report.ReqClosePrice,
                ReqCloseQuantity = report.ReqCloseQuantity,
                ReqOpenPrice = report.ReqOpenPrice,
                SrcAssetAmount = report.SrcAssetAmount,
                SrcAssetCurrency = report.SrcAssetCurrency,
                SrcAssetMovement = report.SrcAssetMovement,
                SrcAssetToUsdConversionRate = report.SrcAssetToUsdConversionRate,
                TradeRecordSide = Convert(report.TradeRecordSide) ?? Api.OrderSide.Buy,
                TradeRecordType =  Convert(report.TradeRecordType) ?? Api.OrderType.Market,
                ReqOpenQuantity = report.ReqOpenQuantity,
                StopPrice = report.StopPrice,
                Tag = report.Tag,
                TransactionCurrency = report.TransactionCurrency,
                TransactionTime = report.TransactionTime,
                UsdToDstAssetConversionRate = report.UsdToDstAssetConversionRate,
                UsdToMarginCurrencyConversionRate = report.UsdToMarginCurrencyConversionRate,
                UsdToProfitCurrencyConversionRate = report.UsdToProfitCurrencyConversionRate,
                UsdToSrcAssetConversionRate = report.UsdToSrcAssetConversionRate,
            };
        }

        public static Api.TradeExecActions Convert(TradeTransactionReportType type)
        {
            switch (type)
            {
                case TradeTransactionReportType.BalanceTransaction: return Api.TradeExecActions.BalanceTransaction;
                case TradeTransactionReportType.Credit: return Api.TradeExecActions.Credit;
                case TradeTransactionReportType.OrderActivated: return Api.TradeExecActions.OrderActivated;
                case TradeTransactionReportType.OrderCanceled: return Api.TradeExecActions.OrderCanceled;
                case TradeTransactionReportType.OrderExpired: return Api.TradeExecActions.OrderExpired;
                case TradeTransactionReportType.OrderFilled: return Api.TradeExecActions.OrderFilled;
                case TradeTransactionReportType.OrderOpened: return Api.TradeExecActions.OrderOpened;
                case TradeTransactionReportType.PositionClosed: return Api.TradeExecActions.PositionClosed;
                case TradeTransactionReportType.PositionOpened: return Api.TradeExecActions.PositionOpened;
                default: return Api.TradeExecActions.None;
            }
        }

        public static Api.TradeRecordTypes GetRecordType(TradeTransactionReport rep)
        {
            if (rep.TradeTransactionReportType == TradeTransactionReportType.BalanceTransaction)
            {
                if (rep.TransactionAmount >= 0)
                    return Api.TradeRecordTypes.Deposit;
                else
                    return Api.TradeRecordTypes.Withdrawal;
            }
            else if (rep.TradeTransactionReportType == TradeTransactionReportType.Credit)
            {
                return Api.TradeRecordTypes.Unknown;
            }
            else if (rep.TradeRecordType == TradeRecordType.Limit || rep.TradeRecordType == TradeRecordType.IoC)
            {
                if (rep.TradeRecordSide == TradeRecordSide.Buy)
                    return Api.TradeRecordTypes.BuyLimit;
                else if (rep.TradeRecordSide == TradeRecordSide.Sell)
                    return Api.TradeRecordTypes.SellLimit;
            }
            else if (rep.TradeRecordType == TradeRecordType.Position || rep.TradeRecordType == TradeRecordType.Market || rep.TradeRecordType == TradeRecordType.MarketWithSlippage)
            {
                if (rep.TradeRecordSide == TradeRecordSide.Buy)
                    return Api.TradeRecordTypes.Buy;
                else if (rep.TradeRecordSide == TradeRecordSide.Sell)
                    return Api.TradeRecordTypes.Sell;
            }
            else if (rep.TradeRecordType == TradeRecordType.Stop)
            {
                if (rep.TradeRecordSide == TradeRecordSide.Buy)
                    return Api.TradeRecordTypes.BuyStop;
                else if (rep.TradeRecordSide == TradeRecordSide.Sell)
                    return Api.TradeRecordTypes.SellStop;
            }

            return Api.TradeRecordTypes.Unknown;
        }

        public static PriceType Convert(Api.BarPriceType priceType)
        {
            switch (priceType)
            {
                case Api.BarPriceType.Ask: return PriceType.Ask;
                case Api.BarPriceType.Bid: return PriceType.Bid;
            }
            throw new NotImplementedException("Unsupported price type: " + priceType);
        }

        public static ExecutionReport Convert(SoftFX.Extended.ExecutionReport report)
        {
            return new ExecutionReport()
            {
                OrderId = report.OrderId,
                // ExecTime = report.???
                Expiration = report.Expiration,
                Created = report.Created,
                Modified = report.Modified,
                RejectReason = Convert(report.RejectReason, report.Text),
                TakeProfit = report.TakeProfit,
                StopLoss = report.StopLoss,
                Text = report.Text,
                Comment = report.Comment,
                Tag = report.Tag,
                Magic = report.Magic,
                IsReducedOpenCommission = report.IsReducedOpenCommission,
                IsReducedCloseCommission = report.IsReducedCloseCommission,
                ImmediateOrCancel= report.ImmediateOrCancel,
                MarketWithSlippage = report.MarketWithSlippage,
                ClosePositionRequestId = report.ClosePositionRequestId,
                TradePrice = report.TradePrice,
                Assets = report.Assets.Select(Convert).ToArray(),
                StopPrice = report.StopPrice,
                AveragePrice = report.AveragePrice,
                ClientOrderId = report.ClientOrderId,
                TradeRequestId = report.TradeRequestId, 
                OrderStatus = Convert(report.OrderStatus),
                ExecutionType = Convert(report.ExecutionType),
                Symbol = report.Symbol,
                ExecutedVolume = report.ExecutedVolume,
                InitialVolume = report.InitialVolume,
                LeavesVolume = report.LeavesVolume,
                MaxVisibleVolume  = report.MaxVisibleVolume,
                TradeAmount = report.TradeAmount,
                Commission = report.Commission,
                AgentCommission = report.AgentCommission,
                Swap = report.Swap,
                InitialOrderType = Convert(report.InitialOrderType) ?? Api.OrderType.Market,
                OrderType = Convert(report.OrderType) ?? Api.OrderType.Market,
                OrderSide = Convert(report.OrderSide) ?? Api.OrderSide.Buy,
                Price = report.Price,
                Balance = report.Balance
            };
        }

        public static ExecutionType Convert(SoftFX.Extended.ExecutionType type)
        {
            return (ExecutionType)type;
        }

        public static OrderStatus Convert(SoftFX.Extended.OrderStatus status)
        {
            return (OrderStatus)status;
        }

        public static BalanceOperationReport Convert(SoftFX.Extended.BalanceOperation op)
        {
            return new BalanceOperationReport(op.Balance, op.TransactionCurrency, op.TransactionAmount);
        }
    }
}
