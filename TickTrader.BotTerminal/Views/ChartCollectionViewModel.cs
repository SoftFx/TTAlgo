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
        private AlgoRepositoryModel repository;

        public ChartCollectionViewModel(ConnectionModel model, AlgoRepositoryModel repository)
        {
            this.model = model;
            this.repository = repository;
        }

        public void Open(string symbol)
        {
            ActivateItem(new ChartViewModel(symbol, model, repository));
        }
    }
}
