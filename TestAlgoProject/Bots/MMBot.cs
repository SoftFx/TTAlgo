using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TestAlgoProject.Bots
{
    [TradeBot(DisplayName = "Market Maker")]
    public class MMBot : TradeBot
    {
        [Parameter(DisplayName = "Configuration File")]
        [FileFilter("Doc files (*.docx)", "*.docx")]
        public File Config { get; set; }

        [Parameter]
        public int P1 { get; set; }

        [Parameter]
        public int P2 { get; set; }
    }
}
