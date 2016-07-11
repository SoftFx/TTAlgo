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
        private PluginCatalog catalog;
        private OrderUi orderUi;

        public ChartCollectionViewModel(FeedModel feedProvider, PluginCatalog catalog, IWindowManager wndManager, OrderUi orderUi)
        {
            this.feed = feedProvider;
            this.catalog = catalog;
            this.wndManager = wndManager;
            this.orderUi = orderUi;
        }

        public void Open(string symbol)
        {
            ActivateItem(new ChartViewModel(symbol, feed, catalog, wndManager, orderUi));
        }

        public void CloseItem(IScreen chart)
        {
            chart.TryClose();
        }
    }
}
