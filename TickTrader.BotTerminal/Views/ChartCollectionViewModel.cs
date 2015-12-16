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
        private FeedModel feed;
        private AlgoRepositoryModel repository;

        public ChartCollectionViewModel(FeedModel feedProvider, AlgoRepositoryModel repository, IWindowManager wndManager)
        {
            this.feed = feedProvider;
            this.repository = repository;
            this.wndManager = wndManager;
        }

        public void Open(string symbol)
        {
            ActivateItem(new ChartViewModel(symbol, feed, repository, wndManager));
        }

        public void CloseItem(IScreen chart)
        {
            chart.TryClose();
        }
    }
}
