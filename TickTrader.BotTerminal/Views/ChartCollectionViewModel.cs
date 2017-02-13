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
        private IShell shell;
        private readonly AlgoEnvironment algoEnv;

        public ChartCollectionViewModel(TraderClientModel clientModel, IShell shell, AlgoEnvironment algoEnv)
        {
            this.clientModel = clientModel;
            this.algoEnv = algoEnv;
            this.shell = shell;

            clientModel.Symbols.Updated += Symbols_Updated;
        }

        private void Symbols_Updated(DictionaryUpdateArgs<string, SymbolModel> args)
        {
            if (args.Action == DLinqAction.Remove)
            {
                var toRemove = Items.Where(i => i.Symbol == args.OldItem.Name).ToList();

                foreach (var chart in toRemove)
                    CloseItem(chart);
            }
        }

        public override void ActivateItem(ChartViewModel item)
        {
            base.ActivateItem(item);
            NotifyOfPropertyChange(nameof(SelectedChartProxy));
        }

        public object SelectedChartProxy
        {
            get { return this.ActiveItem; }
            set
            {
                var chart = value as ChartViewModel;
                if (chart != null)
                    this.ActiveItem = chart;
            }
        }

        public void Open(string symbol)
        {
            ActivateItem(new ChartViewModel(symbol, shell, clientModel, algoEnv));
        }

        public void CloseItem(ChartViewModel chart)
        {
            chart.TryClose();
        }
    }
}
