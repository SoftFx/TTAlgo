using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.Core
{
    internal class DataSeriesProxy : DataSeriesProxy<double>, Api.DataSeries
    {
        public override double DefaultValue { get { return double.NaN; } }
    }

    internal class TimeSeriesProxy : DataSeriesProxy<DateTime>, Api.TimeSeries
    {
        public override DateTime DefaultValue { get { return DateTime.MinValue; } }
    }

    internal class BarSeriesProxy : DataSeriesProxy<Bar>, Api.BarSeries
    {
        private ProxyBuffer<Bar, double> openBuffer = new ProxyBuffer<Bar, double>(b => b.Open);
        private ProxyBuffer<Bar, double> closeBuffer = new ProxyBuffer<Bar, double>(b => b.Close);
        private ProxyBuffer<Bar, double> highBuffer = new ProxyBuffer<Bar, double>(b => b.High);
        private ProxyBuffer<Bar, double> lowBuffer = new ProxyBuffer<Bar, double>(b => b.Low);
        private ProxyBuffer<Bar, double> medianBuffer = new ProxyBuffer<Bar, double>(b => (b.High + b.Low) / 2);
        private ProxyBuffer<Bar, double> volumeBuffer = new ProxyBuffer<Bar, double>(b => b.Volume);
        private ProxyBuffer<Bar, DateTime> openTimeBuffer = new ProxyBuffer<Bar, DateTime>(b => b.OpenTime);

        public BarSeriesProxy()
        {
            Open = new DataSeriesProxy() { Buffer = openBuffer };
            Close = new DataSeriesProxy() { Buffer = closeBuffer };
            High = new DataSeriesProxy() { Buffer = highBuffer };
            Low = new DataSeriesProxy() { Buffer = lowBuffer };
            Mean = new DataSeriesProxy() { Buffer = medianBuffer };
            Volume = new DataSeriesProxy() { Buffer = volumeBuffer };
            OpenTime = new TimeSeriesProxy() { Buffer = openTimeBuffer };
            SymbolCode = string.Empty;
        }

        public string SymbolCode { get; set; }

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

        public Api.DataSeries Open { get; private set; }
        public Api.DataSeries Close { get; private set; }
        public Api.DataSeries High { get; private set; }
        public Api.DataSeries Low { get; private set; }
        public Api.DataSeries Mean { get; private set; }
        public Api.DataSeries Volume { get; private set; }
        public Api.TimeSeries OpenTime { get; private set; }
    }
}
