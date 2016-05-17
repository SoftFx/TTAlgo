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
    internal class FdkAdapter
    {
        public static void Convert(IEnumerable<Bar> srcBars, List<BarEntity> dstBars)
        {
            foreach (var bar in srcBars)
                dstBars.Add(Convert(bar));
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

        public static BarPeriod Convert(Api.TimeFrames timeframe)
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
                case Api.TimeFrames.M1: return BarPeriod.M1;
                case Api.TimeFrames.S10: return BarPeriod.S10;
                case Api.TimeFrames.S1: return BarPeriod.S1;
                
                default: throw new ArgumentException("Cannot convert to BarPeriod: " + timeframe);
            }
        }
    }
}
