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
        private AlgoCatalog catalog;

        public ChartCollectionViewModel(FeedModel feedProvider, AlgoCatalog catalog, IWindowManager wndManager)
        {
            this.feed = feedProvider;
            this.catalog = catalog;
            this.wndManager = wndManager;
        }

        public void Open(string symbol)
        {
            ActivateItem(new ChartViewModel(symbol, feed, catalog, wndManager));
        }

        public void CloseItem(IScreen chart)
        {
            chart.TryClose();
        }
    }
}
