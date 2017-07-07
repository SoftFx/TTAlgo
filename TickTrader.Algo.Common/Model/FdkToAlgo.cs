using SoftFX.Extended;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Api = TickTrader.Algo.Api;
using TickTrader.Algo.Core;
using BO = TickTrader.BusinessObjects;

namespace TickTrader.Algo.Common.Model
{
    public class FdkToAlgo
    {
        public static void Convert(IEnumerable<Bar> srcBars, List<BarEntity> dstBars)
        {
            foreach (var bar in srcBars)
                dstBars.Add(Convert(bar));
        }

        public static IEnumerable<BarEntity> Convert(IEnumerable<Bar> fdkBarCollection)
        {
            return fdkBarCollection.Select(Convert);
        }

        public static IEnumerable<QuoteEntity> Convert(IEnumerable<Quote> fdkQuoteArray)
        {
            return fdkQuoteArray.Select(Convert);
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
            var entity = new QuoteEntity();

            if (fdkTick.HasAsk)
            {
                entity.Ask = fdkTick.Ask;
                entity.AskList = ConvertLevel2(fdkTick.Asks);
            }
            else
            {
                entity.Ask = double.NaN;
                entity.AskList = QuoteEntity.EmptyBook;
            }

            if (fdkTick.HasBid)
            {
                entity.Bid = fdkTick.Bid;
                entity.BidList = ConvertLevel2(fdkTick.Bids);
            }
            else
            {
                entity.Bid = double.NaN;
                entity.BidList = QuoteEntity.EmptyBook;
            }

            entity.Symbol = fdkTick.Symbol;
            entity.Time = fdkTick.CreatingTime;

            return entity;
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
                SortOrder = 0 // ??
            };
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

        public static CurrencyEntity Convert(CurrencyInfo info)
        {
            return new CurrencyEntity(info.Name)
            {
                Digits = info.Precision,
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

        public static Api.OrderType Convert(TradeRecordType fdkType)
        {
            switch (fdkType)
            {
                case TradeRecordType.Limit: return Algo.Api.OrderType.Limit;
                case TradeRecordType.Market: return Algo.Api.OrderType.Market;
                case TradeRecordType.Position: return Algo.Api.OrderType.Position;
                case TradeRecordType.Stop: return Algo.Api.OrderType.Stop;
                case TradeRecordType.StopLimit: return Algo.Api.OrderType.StopLimit;

                default: throw new ArgumentException("Unsupported order type: " + fdkType);
            }
        }

        public static Api.OrderSide Convert(TradeRecordSide fdkSide)
        {
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
                        break;
                    }
                case RejectReason.None:
                    {
                        if (message.StartsWith("Order Not Found"))
                            return Api.OrderCmdResultCodes.OrderNotFound;
                        break;
                    }
            }
            return Api.OrderCmdResultCodes.UnknownError;
        }
    }
}
