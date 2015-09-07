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
        public enum States { Offline, Online }

        public TraderModel()
        {
            Connection = new ConnectionModel();
            Symbols = new SymbolCollectionModel(Connection);
        }

        public ConnectionModel Connection { get; private set; }
        public SymbolCollectionModel Symbols { get; private set; }
    }
}
