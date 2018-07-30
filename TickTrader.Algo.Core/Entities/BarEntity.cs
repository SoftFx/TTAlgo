using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.Core
{
    [Serializable]
    public class BarEntity : Api.Bar, Api.Ext.IBarWriter
    {
        public static readonly BarEntity Empty = new BarEntity() { IsNull = true, Open = double.NaN, Close = double.NaN, High = double.NaN , Low = double.NaN, Volume = double.NaN };

        public BarEntity()
        {
        }

        public BarEntity(DateTime openTime, DateTime closeTime, double price, double volume)
        {
            OpenTime = openTime;
            CloseTime = closeTime;
            Open = price;
            Close = price;
            High = price;
            Low = price;
            Volume = volume;
        }

        public BarEntity(BarEntity original)
        {
            OpenTime = original.OpenTime;
            CloseTime = original.CloseTime;
            Open = original.Open;
            Close = original.Close;
            High = original.High;
            Low = original.Low;
        }

        public double Open { get; set; }
        public double Close { get; set; }
        public double High { get; set; }
        public double Low { get; set; }
        public double Volume { get; set; }
        public DateTime OpenTime { get; set; }
        public DateTime CloseTime { get; set; }
        public bool IsNull { get; set; }

        public BarEntity Clone()
        {
            return new BarEntity(this);
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

        internal void AppendNanProof(double price, double volume)
        {
            Close = price;
            if (double.IsNaN(High) || price > High)
                High = price;
            if (double.IsNaN(Low) || price < Low)
                Low = price;
            if (double.IsNaN(Open))
                Open = price;
            if (double.IsNaN(Volume))
                Volume = volume;
            else
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

        public BarEntity CopyAndAppend(double price, double volume)
        {
            var clone = Clone();
            clone.Append(price, volume);
            return clone;
        }
    }

    public class FullBar
    {
        public FullBar(Bar bid, Bar ask)
        {
        }

        public Bar Bid { get; private set; }
        public Bar Ask { get; private set; }
    }
}
