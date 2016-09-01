using SoftFX.Extended;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Api = TickTrader.Algo.Api;
using TickTrader.Algo.Core;

namespace TickTrader.BotTerminal
{
    internal class FdkToAlgo
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

            entity.SymbolCode = fdkTick.Symbol;
            entity.Time = fdkTick.CreatingTime;

            return entity;
        }

        private static IReadOnlyList<Api.BookEntry> ConvertLevel2(QuoteEntry[] book)
        {
            if (book == null || book.Length == 0)
                return QuoteEntity.EmptyBook;
            else
                return book.Select(Convert).ToList().AsReadOnly();
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
                MinAmount = info.MinTradeVolume,
                MaxAmount = info.MaxTradeVolume,
                BaseCurrencyCode = info.Currency,
                CounterCurrencyCode = info.SettlementCurrency,
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

        public static Api.OrderTypes Convert(TradeRecordType fdkType)
        {
            switch (fdkType)
            {
                case TradeRecordType.Limit: return Algo.Api.OrderTypes.Limit;
                case TradeRecordType.Market: return Algo.Api.OrderTypes.Market;
                case TradeRecordType.Position: return Algo.Api.OrderTypes.Position;
                case TradeRecordType.Stop: return Algo.Api.OrderTypes.Stop;

                default: throw new ArgumentException("Unsupported order type: " + fdkType);
            }
        }

        public static Api.OrderSides Convert(TradeRecordSide fdkSide)
        {
            switch (fdkSide)
            {
                case TradeRecordSide.Buy: return Api.OrderSides.Buy;
                case TradeRecordSide.Sell: return Api.OrderSides.Sell;

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
    }
}
