using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.BotTerminal
{
    class SymbolViewModel
    {
        public SymbolViewModel(string name, string group)
        {
            this.Name = name;
            this.Group = group;
        }

        public string Name { get; private set; }
        public string Group { get; private set; }
    }
}
