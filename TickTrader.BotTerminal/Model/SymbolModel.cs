using SoftFX.Extended;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.BotTerminal.Lib;

namespace TickTrader.BotTerminal
{
    internal class SymbolModel
    {
        private object lockObj = new object();
        private SymbolCollectionModel container;
        private List<IRateUpdatesListener> listeners = new List<IRateUpdatesListener>();

        public SymbolModel(SymbolCollectionModel container, SymbolInfo info)
        {
            this.container = container;
            this.Descriptor = info;
            this.Amounts = new OrderAmountModel(info);
            this.PredefinedAmounts = Amounts.GetPredefined();
        }

        public string Name { get { return Descriptor.Name; } }
        public SymbolInfo Descriptor { get; private set; }
        public int Depth { get; private set; }
        public int RequestedDepth { get; private set; }
        public Quote LastQuote { get; private set; }
        public OrderAmountModel Amounts { get; private set; }
        public List<decimal> PredefinedAmounts { get; private set; }

        public event Action<SymbolInfo> InfoUpdated = delegate { };

        public void Update(SymbolInfo newInfo)
        {
            this.Descriptor = newInfo;
            InfoUpdated(newInfo);
        }

        public void CastNewTick(Quote tick)
        {
            listeners.ForEach(l => l.OnRateUpdate(tick));
            LastQuote = tick;
        }

        public void Subscribe(IRateUpdatesListener listener)
        {
            this.listeners.Add(listener);
            listener.DepthChanged += listener_DepthChanged;
            UpdateRequestedSubscription();
        }

        public void Unsubscribe(IRateUpdatesListener listener)
        {
            this.listeners.Remove(listener);
            listener.DepthChanged -= listener_DepthChanged;
            UpdateRequestedSubscription();
        }

        public bool ValidateAmmount(decimal amount, decimal minVolume, decimal maxVolume, decimal step)
        {
            return amount <= maxVolume && amount >= minVolume
                && (amount / step) % 1 == 0;
        }

        public void Reset()
        {
            Depth = RequestedDepth;
        }

        private void UpdateRequestedSubscription()
        {
            if (listeners.Count == 0)
                RequestedDepth = 1;
            else
                RequestedDepth = GetMaxDepth();

            if (RequestedDepth != Depth)
            {
                container.EnqueueSubscriptionRequest(RequestedDepth, Name);
                Depth = RequestedDepth;
            }
        }

        private int GetMaxDepth()
        {
            int max = 0;
            foreach (var l in listeners)
            {
                if (l.Depth == 0)
                    return 0;
                if (l.Depth > max)
                    max = l.Depth;
            }
            return max;
        }

        private void listener_DepthChanged()
        {
            UpdateRequestedSubscription();
        }
    }

    internal interface IRateUpdatesListener
    {
        int Depth { get; }
        event Action DepthChanged;
        void OnRateUpdate(Quote tick);
    }
}
