using System;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.CoreV1
{
    public class BarEntity : Api.Bar, Api.Ext.IBarWriter
    {
        private readonly BarData _data;

        public static BarEntity Empty => new BarEntity(null);

        public BarEntity()
        {
        }

        public BarEntity(BarData data)
        {
            _data = data ?? BarData.CreateEmpty();
            IsNull = data == null;
        }

        public double Open
        {
            get { return _data.Open; }
            set { _data.Open = value; }
        }
        public double Close
        {
            get { return _data.Close; }
            set { _data.Close = value; }
        }
        public double High
        {
            get { return _data.High; }
            set { _data.High = value; }
        }
        public double Low
        {
            get { return _data.Low; }
            set { _data.Low = value; }
        }
        public double Volume
        {
            get { return _data.RealVolume; }
            set { _data.RealVolume = value; }
        }
        public DateTime OpenTime => TimeMs.ToUtc(_data.OpenTime);
        public DateTime CloseTime => TimeMs.ToUtc(_data.CloseTime);
        public bool IsNull { get; }

        public BarEntity Clone()
        {
            return new BarEntity(_data.Clone());
        }

        public void Append(double price, double volume)
        {
            Close = price;
            if (price > High)
                High = price;
            if (price < Low)
                Low = price;
            Volume += volume;
        }

        public void AppendPart(double open, double high, double low, double close, double volume)
        {
            High = System.Math.Max(High, high);
            Low = System.Math.Min(Low, low);
            Close = close;
            Volume += volume;
        }

        public void Append(BarEntity anotherBar)
        {
            Close = anotherBar.Close;
            if (anotherBar.High > High)
                High = anotherBar.High;
            if (anotherBar.Low < Low)
                Low = anotherBar.Low;
            Volume += anotherBar.Volume;
        }
    }
}
