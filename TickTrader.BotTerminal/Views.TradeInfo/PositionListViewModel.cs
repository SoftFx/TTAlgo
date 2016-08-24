using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.BotTerminal
{
    class PositionListViewModel: PropertyChangedBase
    {
        public PositionListViewModel(NetPositionListViewModel netPositions, GrossPositionListViewModel grossPositions)
        {
            Net = netPositions;
            Gross = grossPositions;
        }

        public NetPositionListViewModel Net { get; }
        public GrossPositionListViewModel Gross { get; }
        public bool IsEnabled { get; set; }
    }
}
