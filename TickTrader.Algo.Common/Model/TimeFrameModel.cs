using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.Common.Model
{
    public static class TimeFrameModel
    {
        public static readonly TimeFrames[] AllTimeFrames = new TimeFrames[]
        {
            TimeFrames.MN,
            TimeFrames.W,
            TimeFrames.D,
            TimeFrames.H4,
            TimeFrames.H1,
            TimeFrames.M30,
            TimeFrames.M15,
            TimeFrames.M5,
            TimeFrames.M1,
            TimeFrames.S10,
            TimeFrames.S1,
            TimeFrames.Ticks,
            TimeFrames.TicksLevel2
        };

        public static readonly TimeFrames[] BarTimeFrames = new TimeFrames[]
        {
            TimeFrames.MN,
            TimeFrames.W,
            TimeFrames.D,
            TimeFrames.H4,
            TimeFrames.H1,
            TimeFrames.M30,
            TimeFrames.M15,
            TimeFrames.M5,
            TimeFrames.M1,
            TimeFrames.S10,
            TimeFrames.S1,
        };

    }
}
