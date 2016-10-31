using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotCommon
{
    [Flags]
    public enum OrderKeeperSettings
    {
        None = 0,
        AutoAddVolume2PartialFill = 1,
        AutoUpdate2TradeEvent = 2
    }
}
