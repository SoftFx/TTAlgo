using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.BotTerminal
{
    internal class BotListOverlayViewModel
    {
        public BotListOverlayViewModel(IReadOnlyList<AlgoBotViewModel> botList)
        {
            Bots = botList;
        }

        public IReadOnlyList<AlgoBotViewModel> Bots { get; }
    }
}
