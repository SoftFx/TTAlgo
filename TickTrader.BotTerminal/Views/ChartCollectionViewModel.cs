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
        private PluginCatalog catalog;
        private IShell shell;
        private BotJournal pluginJournal;

        public ChartCollectionViewModel(FeedModel feedProvider, PluginCatalog catalog, IShell shell, BotJournal pluginJournal)
        {
            this.feed = feedProvider;
            this.catalog = catalog;
            this.shell = shell;
            this.pluginJournal = pluginJournal;
        }

        public void Open(string symbol)
        {
            ActivateItem(new ChartViewModel(symbol, shell, feed, catalog, pluginJournal));
        }

        public void CloseItem(ChartViewModel chart)
        {
            chart.TryClose();
        }
    }
}
