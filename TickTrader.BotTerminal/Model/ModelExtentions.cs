using Machinarium.Var;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TickTrader.BotTerminal
{
    internal static class ModelExtentions
    {
        public static bool IsTicks(this TimeFrames timeFrame)
        {
            return timeFrame == TimeFrames.Ticks || timeFrame == TimeFrames.TicksLevel2;
        }

        public static BoolVar IsTicks(this Var<TimeFrames> timeFrame)
        {
            return timeFrame.Check(IsTicks);
        }
    }
}
