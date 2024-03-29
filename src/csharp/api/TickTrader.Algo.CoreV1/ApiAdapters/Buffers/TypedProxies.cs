﻿using System;
using System.Threading;
using TickTrader.Algo.Api;
using TickTrader.Algo.Core;

namespace TickTrader.Algo.CoreV1
{
    internal class DataSeriesProxy : DataSeriesImpl<double>, DataSeries
    {
    }

    internal class TimeSeriesProxy : DataSeriesImpl<DateTime>, TimeSeries
    {
    }

    internal class MarkerSeriesProxy : DataSeriesImpl<Marker>, MarkerSeries
    {
    }

    internal class QuoteSeriesProxy : DataSeriesImpl<Quote>, QuoteSeries, QuoteL2Series
    {
        private Lazy<DataSeriesProxy> askBufferLazy;
        private Lazy<DataSeriesProxy> bidBufferLazy;
        private Lazy<TimeSeriesProxy> timeBufferLazy;

        public QuoteSeriesProxy()
        {
            askBufferLazy = ConstructLazy<DataSeriesProxy, double>(q => q.Ask);
            bidBufferLazy = ConstructLazy<DataSeriesProxy, double>(q => q.Ask);
            timeBufferLazy = ConstructLazy<TimeSeriesProxy, DateTime>(q => q.Time);
        }

        private Lazy<TProxy> ConstructLazy<TProxy, TData>(Func<Quote, TData> selector) where TProxy : DataSeriesImpl<TData>, new()
        {
            return new Lazy<TProxy>(() =>
            {
                var seriesProxy = new TProxy();
                seriesProxy.Buffer = new ProxyBuffer<Quote, TData>(selector) { SrcBuffer = Buffer };
                return seriesProxy;
            }, LazyThreadSafetyMode.None);
        }

        public DataSeries Ask { get { return askBufferLazy.Value; } }
        public DataSeries Bid { get { return bidBufferLazy.Value; } }
        public TimeSeries Time { get { return timeBufferLazy.Value; } }
    }

    internal class BarSeriesProxy : DataSeriesImpl<Bar>, BarSeries
    {
        private ProxyBuffer<Bar, double> openBuffer = new ProxyBuffer<Bar, double>(b => b.Open);
        private ProxyBuffer<Bar, double> closeBuffer = new ProxyBuffer<Bar, double>(b => b.Close);
        private ProxyBuffer<Bar, double> highBuffer = new ProxyBuffer<Bar, double>(b => b.High);
        private ProxyBuffer<Bar, double> lowBuffer = new ProxyBuffer<Bar, double>(b => b.Low);
        private ProxyBuffer<Bar, double> medianBuffer = new ProxyBuffer<Bar, double>(b => (b.High + b.Low) / 2);
        private ProxyBuffer<Bar, double> volumeBuffer = new ProxyBuffer<Bar, double>(b => b.Volume);
        private ProxyBuffer<Bar, DateTime> openTimeBuffer = new ProxyBuffer<Bar, DateTime>(b => b.OpenTime);
        private Lazy<DataSeriesProxy> typicalBufferLazy;
        private Lazy<DataSeriesProxy> weightedBufferLazy;
        private Lazy<DataSeriesProxy> moveBufferLazy;
        private Lazy<DataSeriesProxy> rangeBufferLazy;

        public BarSeriesProxy()
        {
            Open = new DataSeriesProxy() { Buffer = openBuffer };
            Close = new DataSeriesProxy() { Buffer = closeBuffer };
            High = new DataSeriesProxy() { Buffer = highBuffer };
            Low = new DataSeriesProxy() { Buffer = lowBuffer };
            Median = new DataSeriesProxy() { Buffer = medianBuffer };
            Volume = new DataSeriesProxy() { Buffer = volumeBuffer };
            OpenTime = new TimeSeriesProxy() { Buffer = openTimeBuffer };

            typicalBufferLazy = ConstructLazy<DataSeriesProxy, double>(b => (b.High + b.Low + b.Close) / 3);
            weightedBufferLazy = ConstructLazy<DataSeriesProxy, double>(b => (b.High + b.Low + b.Close * 2) / 4);
            moveBufferLazy = ConstructLazy<DataSeriesProxy, double>(b => b.Close - b.Open);
            rangeBufferLazy = ConstructLazy<DataSeriesProxy, double>(b => b.High - b.Low);

            Symbol = string.Empty;
        }

        private Lazy<TProxy> ConstructLazy<TProxy, TData>(Func<Bar, TData> selector) where TProxy : DataSeriesImpl<TData>, new()
        {
            return new Lazy<TProxy>(() =>
            {
                var seriesProxy = new TProxy();
                seriesProxy.Buffer = new ProxyBuffer<Bar, TData>(selector) { SrcBuffer = Buffer };
                return seriesProxy;
            }, LazyThreadSafetyMode.None);
        }

        public string Symbol { get; set; }

        public override IPluginDataBuffer<Bar> Buffer
        {
            get { return base.Buffer; }
            set
            {
                base.Buffer = value;
                openBuffer.SrcBuffer = value;
                closeBuffer.SrcBuffer = value;
                highBuffer.SrcBuffer = value;
                lowBuffer.SrcBuffer = value;
                medianBuffer.SrcBuffer = value;
                volumeBuffer.SrcBuffer = value;
                openTimeBuffer.SrcBuffer = value;
            }
        }

        public DataSeries Open { get; private set; }
        public DataSeries Close { get; private set; }
        public DataSeries High { get; private set; }
        public DataSeries Low { get; private set; }
        public DataSeries Median { get; private set; }
        public DataSeries Volume { get; private set; }
        public TimeSeries OpenTime { get; private set; }

        public DataSeries Typical { get { return typicalBufferLazy.Value; } }
        public DataSeries Weighted { get { return weightedBufferLazy.Value; } }
        public DataSeries Move { get { return moveBufferLazy.Value; } }
        public DataSeries Range { get { return rangeBufferLazy.Value; } }
    }
}
