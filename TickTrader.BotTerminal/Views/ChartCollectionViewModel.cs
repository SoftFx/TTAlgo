using Caliburn.Micro;
using Machinarium.Qnil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.BotTerminal
{
    internal class ChartCollectionViewModel : Conductor<ChartViewModel>.Collection.OneActive
    {
        private TraderClientModel clientModel;
        private PluginCatalog catalog;
        private IShell shell;
        private BotJournal pluginJournal;

        public ChartCollectionViewModel(TraderClientModel clientModel, PluginCatalog catalog, IShell shell, BotJournal pluginJournal)
        {
            this.clientModel = clientModel;
            this.catalog = catalog;
            this.shell = shell;
            this.pluginJournal = pluginJournal;

            clientModel.Symbols.Updated += Symbols_Updated;
        }

        private void Symbols_Updated(DictionaryUpdateArgs<string, SymbolModel> args)
        {
            if (args.Action == DLinqAction.Remove)
            {
                foreach (var chart in Items)
                {
                    if (chart.Symbol == args.OldItem.Name)
                        CloseItem(chart);
                }
            }
        }

        public void Open(string symbol)
        {
            ActivateItem(new ChartViewModel(symbol, shell, clientModel, catalog, pluginJournal));
        }

        public void CloseItem(ChartViewModel chart)
        {
            chart.TryClose();
        }
    }
}
