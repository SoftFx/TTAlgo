using Google.Protobuf.WellKnownTypes;
using System;
using System.Collections.Generic;
using TickTrader.Algo.Core;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Backtester
{
    class BarSeriesReader : SeriesReader
    {
        private string _symbol;
        private IEnumerable<BarData> _bidSrc;
        private IEnumerable<BarData> _askSrc;

        private IEnumerator<BarData> _bidE;
        private IEnumerator<BarData> _askE;

        private Timestamp _lastBarTime;
        private BarData _lastBid;
        private BarData _lastAsk;
        private Feed.Types.Timeframe _baseTimeFrame;

        //private IBarStorage _bidStorage;
        //private IBarStorage _askStorage;

        public BarSeriesReader(string symbol, Feed.Types.Timeframe baseTimeFrame, IBarStorage bidSrc, IBarStorage askSrc)
            : this(symbol, baseTimeFrame, bidSrc?.GetBarStream(), askSrc?.GetBarStream())
        {
            //_bidStorage = bidSrc;
            //_askStorage = askSrc;
        }

        public BarSeriesReader(string symbol, Feed.Types.Timeframe baseTimeFrame, IEnumerable<BarData> bidSrc, IEnumerable<BarData> askSrc)
        {
            _symbol = symbol;
            _bidSrc = bidSrc;
            _askSrc = askSrc;

            if (bidSrc == null && askSrc == null)
                throw new InvalidOperationException("Both ask and bid streams are null!");

            _baseTimeFrame = baseTimeFrame;

            // add base builders

            //if (bidSrc != null)
            //    GetOrAddBuilder(BarPriceType.Bid, baseTimeFrame);

            //if (askSrc != null)
            //    GetOrAddBuilder(BarPriceType.Ask, baseTimeFrame);
        }

        public override void Start()
        {
            if (_bidE == null)
            {
                _bidE = _bidSrc?.GetEnumerator();
                _askE = _askSrc?.GetEnumerator();

                if (_bidE != null)
                    MoveBid();

                if (_askE != null)
                    MoveAsk();
            }
        }

        public override void Stop()
        {
            _bidE?.Dispose();
            _askE?.Dispose();
        }

        private void MoveBid()
        {
            if (!_bidE.MoveNext())
            {
                _bidE.Dispose();
                _bidE = null;
            }
        }

        private void MoveAsk()
        {
            if (!_askE.MoveNext())
            {
                _askE.Dispose();
                _askE = null;
            }
        }

        private void UpdateBars(IEnumerable<BarVector> collection, BarData bar)
        {
            foreach (var rec in collection)
                rec.AppendBarPart(bar);
        }

        private bool TakeAskBar()
        {
            _lastBarTime = _askE.Current.OpenTime;
            _lastAsk = _askE.Current;
            _lastBid = CreateFiller(_lastAsk.OpenTime, _lastAsk.CloseTime, _lastBid?.Close ?? double.NaN);

            //UpdateBars(_askBars.Values, _askE.Current);

            MoveAsk();

            Current = GetCurrentRate();
            return true;
        }

        private bool TakeBidBar()
        {
            _lastBarTime = _bidE.Current.OpenTime;
            _lastBid = _bidE.Current;
            _lastAsk = CreateFiller(_lastBid.OpenTime, _lastBid.CloseTime, _lastAsk?.Close ?? double.NaN);

            //UpdateBars(_bidBars.Values, _bidE.Current);

            MoveBid();

            Current = GetCurrentRate();
            return true;
        }

        private bool TakeBoth()
        {
            _lastBarTime = _bidE.Current.OpenTime;
            _lastBid = _bidE.Current;
            _lastAsk = _askE.Current;

            //UpdateBars(_bidBars.Values, _bidE.Current);
            //UpdateBars(_askBars.Values, _askE.Current);

            MoveBid();
            MoveAsk();

            Current = GetCurrentRate();
            return true;
        }

        public override bool MoveNext()
        {
            if (_bidE != null)
            {
                if (_askE != null)
                {
                    if (_bidE.Current.OpenTime == _askE.Current.OpenTime)
                        return TakeBoth();
                    else if (_bidE.Current.OpenTime < _askE.Current.OpenTime)
                        return TakeBidBar();
                    else
                        return TakeAskBar();
                }
                else
                    return TakeBidBar();
            }
            else if (_askE != null)
                return TakeAskBar();
            else
                return false;
        }

        public override SeriesReader Clone()
        {
            return new BarSeriesReader(_symbol, _baseTimeFrame, _bidSrc, _askSrc);
        }

        protected virtual IRateInfo GetCurrentRate()
        {
            return new BarRateUpdate(_lastBid, _lastAsk, _symbol);
        }

        private BarData CreateFiller(Timestamp barOpenTime, Timestamp barCloseTime, double price)
        {
            return new BarData(barOpenTime, barCloseTime, price, 0);
        }
    }
}
