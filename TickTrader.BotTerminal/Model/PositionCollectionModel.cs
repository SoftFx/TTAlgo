using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.BotTerminal.Model
{
    class PositionCollectionModel
    {
        public enum States { Offline, WatingSnapshot, Online }
        public enum Events { Disconnected, SybmolsArrived, DoneUpdating, DoneStopping }

        public PositionCollectionModel(ConnectionModel connection)
        {
            connection.Initialized += ()=>
                {
                    
                };
        }
    }

    abstract class PositionStrategy
    {
    }

    class OrderPositionStrategy : PositionStrategy
    {
    }

    class SeparatePositionStrategy : PositionStrategy
    {
    }
}
