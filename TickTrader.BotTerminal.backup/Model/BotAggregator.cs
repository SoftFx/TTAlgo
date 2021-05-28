using Machinarium.Qnil;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.BotTerminal
{
    internal class BotAggregator : IBotAggregator
    {
        private ChartCollectionViewModel _charts;

        public BotAggregator(ChartCollectionViewModel charts)
        {
            _charts = charts;
            _charts.Items.CollectionChanged += ChartsCollectionChanged;
        }

        public IEnumerable<TradeBotModel> Items => _charts.Items.SelectMany(chart => chart.Bots.Select(b => b.Model));

        private void ChartsCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                ((IObservableList<BotControlViewModel>)((ChartViewModel)e.NewItems[0]).Bots).CollectionChanged += BotsCollectionChanged;
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                ((IObservableList<BotControlViewModel>)((ChartViewModel)e.OldItems[0]).Bots).CollectionChanged -= BotsCollectionChanged;
            }
        }

        private void BotsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                var tradeBot = ((BotControlViewModel)e.NewItems[0]).Model;
                tradeBot.StateChanged += StateChanged;
                Added?.Invoke(tradeBot);
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                var tradeBot = ((BotControlViewModel)e.OldItems[0]).Model;
                tradeBot.StateChanged -= StateChanged;
                Removed?.Invoke(tradeBot);
            }
        }

        public event Action<TradeBotModel> Added = delegate { };
        public event Action<TradeBotModel> Removed = delegate { };
        public event Action<TradeBotModel> StateChanged = delegate { };
    }

    internal interface IBotAggregator : IAggregator<TradeBotModel>
    {
        event Action<TradeBotModel> StateChanged;
    }

    internal interface IAggregator<T>
    {
        IEnumerable<T> Items { get; }
        event Action<T> Added;
        event Action<T> Removed;
    }
}
