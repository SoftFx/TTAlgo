using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.BotTerminal
{
    internal interface IDropHandler
    {
        void Drop(object o);
        bool CanDrop(object o);
    }
}
