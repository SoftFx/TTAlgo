using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.Core
{
    public class SyncBarVector : BarVector
    {
        public SyncBarVector(TimeFrames timeFrame, ITimeRef syncRef) : base(timeFrame)
        {
        }
    }
}
