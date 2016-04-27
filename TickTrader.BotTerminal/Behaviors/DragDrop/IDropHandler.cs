using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.BotTerminal
{
    public interface IDropHandler
    {
        void Drop(object o);
        Type[] AcceptedTypes { get; }
    }
}
