using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.Core
{
    class BarSeriesReader : SeriesReader
    {
        private string _symbol;
        private IEnumerable<BarEntity> _bidSrc;
        private IEnumerable<BarEntity> _askSrc;

        private IEnumerator<BarEntity> _bidE;
        private IEnumerator<BarEntity> _askE;

        private DateTime _lastBarTime;
        private BarEntity _lastBid;
        private BarEntity _lastAsk;
        private TimeFrames _baseTimeFrame;

        //private IBarStorage _bidStorage;
        //private IBarStorage _askStorage;

        public BarSeriesReader(string symbol, TimeFrames baseTimeFrame, IBarStorage bidSrc, IBarStorage askSrc)
            : this(symbol, baseTimeFrame, bidSrc?.GrtBarStream(), askSrc?.GrtBarStream())
        {
            //_bidStorage = bidSrc;
            //_askStorage = askSrc;
        }

        public BarSeriesReader(string symbol, TimeFrames baseTimeFrame, IEnumerable<BarEntity> bidSrc, IEnumerable<BarEntity> askSrc)
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

        private void UpdateBars(IEnumerable<BarVector> collection, BarEntity bar)
        {
            foreach (var rec in collection)
                rec.AppendBarPart(bar.OpenTime, bar.Open, bar.High, bar.Low, bar.Close, bar.Volume);
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

        protected virtual RateUpdate GetCurrentRate()
        {
            return new BarRateUpdate(_lastBid, _lastAsk, _symbol);
        }

        private BarEntity CreateFiller(DateTime barOpenTime, DateTime barCloseTime, double price)
        {
            return new BarEntity(barOpenTime, barCloseTime, price, 0);
        }
    }
}
