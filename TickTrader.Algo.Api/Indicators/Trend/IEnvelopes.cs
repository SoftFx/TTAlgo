using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Api.Indicators
{
    public interface IEnvelopes
    {
        int Period { get; }

        int Shift { get; }

        double Deviation { get; }

        MovingAverageMethod TargetMethod { get; }

        DataSeries Price { get; }

        DataSeries TopLine { get; }

        DataSeries BottomLine { get; }
    }
}
