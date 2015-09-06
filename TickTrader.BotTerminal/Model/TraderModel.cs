using SoftFX.Extended;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.BotTerminal
{
    internal class TraderModel
    {
        public TraderModel()
        {
            Connection = new ConnectionModel();
            Symbols = new SymbolListModel(Connection);
        }

        public ConnectionModel Connection { get; private set; }
        public SymbolListModel Symbols { get; private set; }
    }
}
