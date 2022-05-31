using LiveChartsCore.Defaults;
using Machinarium.ObservableCollections;
using System;
using System.Linq;
using TickTrader.Algo.Core;
using TickTrader.Algo.Domain;
using TickTrader.BotTerminal.Controls.Chart;

namespace TickTrader.BotTerminal
{
    public sealed class ObservableBarVector : ObservableRangeCollection<FinancialPoint>
    {
        private const int MaxBarVectorSize = 4000;

        private Feed.Types.Timeframe _timeFrame;
        private BarSampler _sampler;

        internal Feed.Types.Timeframe Timeframe
        {
            get => _timeFrame;

            set
            {
                if (_timeFrame == value)
                    return;

                _timeFrame = value;
                _sampler = BarSampler.Get(value);
            }
        }


        internal event Action VectorInitEvent;
        internal event Action NewBarEvent;
        internal event Action<double?, double?> ApplyNewTickEvent;


        internal ObservableBarVector(Feed.Types.Timeframe timeFrame)
        {
            Timeframe = timeFrame;
        }


        public void InitNewVector(BarData[] vector)
        {
            Clear();
            AddRange(vector.Select(b => b.ToPoint()));

            VectorInitEvent?.Invoke();
        }


        public void AppendBar(BarData bar) => AppendBarIntenral(bar, false);

        public bool TryAppendBar(BarData bar) => AppendBarIntenral(bar, true);

        private bool AppendBarIntenral(BarData bar, bool noThrow)
        {
            var boundaries = _sampler.GetBar(bar.OpenTime);
            var currentBar = Items?.LastOrDefault();

            if (currentBar != null && currentBar.Date.Ticks >= boundaries.Open.Value)
                return noThrow ? false : throw new ArgumentException("Invalid time sequnce!");

            if (boundaries.Open != bar.OpenTime || boundaries.Close != bar.CloseTime)
                return noThrow ? false : throw new ArgumentException("Bar has invalid time boundaries!");

            Add(bar.ToPoint());

            if (Count > MaxBarVectorSize)
                RemoveAt(0);

            NewBarEvent?.Invoke();

            return true;
        }


        public void ApplyQuote(QuoteInfo quote)
        {
            if (TryAppendBidQuote(quote))
                ApplyNewTickEvent?.Invoke(quote.DoubleNullableBid(), quote.DoubleNullableAsk());
        }

        public bool TryAppendBidQuote(QuoteInfo quote)
        {
            return quote.HasBid && AppendQuoteInternal(quote.Time, quote.Bid, true);
        }

        public bool TryAppendAskQuote(QuoteInfo quote)
        {
            return quote.HasAsk && AppendQuoteInternal(quote.Time, quote.Ask, true);
        }

        private bool AppendQuoteInternal(UtcTicks time, double price, bool noThrow)
        {
            var boundaries = _sampler.GetBar(time);
            var currentBar = Items?.LastOrDefault();

            if (currentBar != null)
            {
                if (time.Value < currentBar.Date.Ticks)
                    return noThrow ? false : throw new ArgumentException("Invalid time sequnce!");

                if (currentBar.Date.Ticks == boundaries.Open.Value)
                {
                    currentBar.ApplyTick(price);
                    return true;
                }
            }

            return AppendBarIntenral(new BarData(boundaries.Open, boundaries.Close, price, 1), noThrow);
        }
    }
}
