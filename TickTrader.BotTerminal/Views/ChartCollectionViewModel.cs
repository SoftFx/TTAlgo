using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.BotTerminal
{
    internal class ChartCollectionViewModel : Conductor<IScreen>.Collection.OneActive
    {
        private ConnectionModel model;

        public ChartCollectionViewModel(ConnectionModel model)
        {
            this.model = model;
        }

        public void Open(string symbol)
        {
            ActivateItem(new ChartViewModel(symbol, model));
        }
    }
}
