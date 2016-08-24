using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.BotTerminal
{
    internal class ChartCollectionViewModel : Conductor<ChartViewModel>.Collection.OneActive
    {
        private FeedModel feed;
        private TraderModel trade;
        private PluginCatalog catalog;
        private IShell shell;
        private BotJournal pluginJournal;

        public ChartCollectionViewModel(FeedModel feed,  TraderModel trade, PluginCatalog catalog, IShell shell, BotJournal pluginJournal)
        {
            this.feed = feed;
            this.trade = trade;
            this.catalog = catalog;
            this.shell = shell;
            this.pluginJournal = pluginJournal;
        }

        public void Open(string symbol)
        {
            ActivateItem(new ChartViewModel(symbol, shell, feed, trade, catalog, pluginJournal));
        }

        public void CloseItem(ChartViewModel chart)
        {
            chart.TryClose();
        }
    }
}
