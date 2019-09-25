using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotTerminalPorfileGenerator
{
    public class ProfileInfo
    {
        public List<ChartInfo> Charts { get; } = new List<ChartInfo>();
        public string OpenedChartSymbol { get; set; }
    }

    public class ChartInfo
    {
        public string IndicatorNum { get; set; }
        public string ChartId { get; set; }
        public string Symbol { get; set; }
    }
}
