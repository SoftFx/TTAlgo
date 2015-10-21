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
        private IWindowManager wndManager;
        private ConnectionModel model;
        private AlgoRepositoryModel repository;

        public ChartCollectionViewModel(ConnectionModel model, AlgoRepositoryModel repository, IWindowManager wndManager)
        {
            this.model = model;
            this.repository = repository;
            this.wndManager = wndManager;
        }

        public void Open(string symbol)
        {
            ActivateItem(new ChartViewModel(symbol, model, repository, wndManager));
        }

        public void CloseItem(IScreen chart)
        {
            chart.TryClose();
        }
    }
}
