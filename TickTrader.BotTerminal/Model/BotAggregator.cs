using Machinarium.Qnil;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.BotTerminal
{
    internal interface IBotAggregator
    {
        IDynamicListSource<TradeBotModel> Bots { get; }

        IDynamicListSource<BotControlViewModel> BotControls { get; }


        event Action<TradeBotModel> StateChanged;
    }


    internal class BotAggregator : IBotAggregator
    {
        private DynamicList<ChartViewModel> _charts;


        public IDynamicListSource<TradeBotModel> Bots { get; }

        public IDynamicListSource<BotControlViewModel> BotControls { get; }


        public event Action<TradeBotModel> StateChanged;


        public BotAggregator(ChartCollectionViewModel charts)
        {
            _charts = new DynamicList<ChartViewModel>(charts.Items);
            Bots = _charts.SelectMany(c => c.BotsList).Select(bc => bc.Model);
            BotControls = _charts.SelectMany(c => c.BotsList);

            charts.Items.CollectionChanged += ChartsCollectionChanged;
            Bots.Updated += BotsCollectionUpdated;
        }


        private void ChartsCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (ChartViewModel item in e.NewItems)
                {
                    _charts.Add(item);
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (ChartViewModel item in e.OldItems)
                {
                    _charts.Remove(item);
                }
            }
        }

        private void BotsCollectionUpdated(ListUpdateArgs<TradeBotModel> args)
        {
            if (args.Action == DLinqAction.Insert)
            {
                var tradeBot = args.NewItem;
                tradeBot.StateChanged += StateChanged;
            }
            else if (args.Action == DLinqAction.Remove)
            {
                var tradeBot = args.OldItem;
                tradeBot.StateChanged -= StateChanged;
            }
        }
    }
}
