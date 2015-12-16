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

        public TraderModel(ConnectionModel connection)
        {
            Connection = connection;
            Account = new AccountModel(Connection);
        }

        public ConnectionModel Connection { get; private set; }
        
        public AccountModel Account { get; private set; }
    }

    internal class FeedModel
    {
        public FeedModel(ConnectionModel connection)
        {
            this.Connection = connection;
            this.Symbols = new SymbolCollectionModel(connection);
        }

        public ConnectionModel Connection { get; private set; }
        public SymbolCollectionModel Symbols { get; private set; }
        public FeedHistoryProviderModel History { get { return Connection.FeedCache; } }
    }
}
