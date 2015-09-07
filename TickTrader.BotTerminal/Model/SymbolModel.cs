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
        private SymbolCollectionModel container;
        private List<IRateUpdatesListener> listeners = new List<IRateUpdatesListener>();

        public SymbolModel(SymbolCollectionModel container, SymbolInfo info)
        {
            this.container = container;
            this.Descriptor = info;
        }

        public string Name { get { return Descriptor.Name; } }
        public SymbolInfo Descriptor { get; private set; }
        public SubscriptionInfo CurrentSubscription { get; set; }
        public SubscriptionInfo RequestedSubscription { get; private set; }

        public event Action<SymbolInfo> InfoUpdated = delegate { };

        public void Update(SymbolInfo newInfo)
        {
            this.Descriptor = newInfo;
            InfoUpdated(newInfo);
        }

        public void CastNewTick(Quote tick)
        {
            listeners.ForEach(l => l.OnRateUpdate(tick));
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

        private void UpdateRequestedSubscription()
        {
            if (listeners.Count > 0)
                RequestedSubscription = new SubscriptionInfo(false, 1);
            else
                RequestedSubscription = new SubscriptionInfo(true, listeners.Max(l => l.Depth));
        }

        private void listener_DepthChanged()
        {
            UpdateRequestedSubscription();
        }
    }

    public struct SubscriptionInfo
    {
        public SubscriptionInfo(bool subscribed, int depth) : this()
        {
            this.Subscribed = subscribed;
            this.Depth = depth;
        }

        public bool Subscribed { get; private set; }
        public int Depth { get; private set; }

        public static bool operator ==(SubscriptionInfo c1, SubscriptionInfo c2)
        {
            return c1.Equals(c2);
        }

        public static bool operator !=(SubscriptionInfo c1, SubscriptionInfo c2)
        {
            return !c1.Equals(c2);
        }
    }

    internal interface IRateUpdatesListener
    {
        int Depth { get; }
        event Action DepthChanged;
        void OnRateUpdate(Quote tick);
    }
}
