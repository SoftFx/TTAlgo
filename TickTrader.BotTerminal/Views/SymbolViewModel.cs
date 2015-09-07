using Caliburn.Micro;
using SoftFX.Extended;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace TickTrader.BotTerminal
{
    class SymbolViewModel : PropertyChangedBase, IRateUpdatesListener
    {
        private SymbolModel model;

        public SymbolViewModel(SymbolModel model)
        {
            this.model = model;
            this.model.Subscribe(this);
            if (model.Descriptor.Features.IsColorSupported)
                Color = model.Descriptor.Color;
        }

        public string Name { get { return model.Name; } }
        public string Group { get { return "Forex"; } }
        public int Color { get; private set; }

        public double? Bid { get; set; }
        public double? Ask { get; set; }
        public RateChangeDirections BidDirection { get; private set; }
        public RateChangeDirections AskDirection { get; private set; }

        public int Depth { get; private set; }

        public event System.Action DepthChanged = delegate { };

        public void OnRateUpdate(Quote tick)
        {
            if (tick.HasBid)
            {
                BidDirection = GetDirection(Bid, tick.Bid);
                Bid = tick.Bid;
                NotifyOfPropertyChange("Bid");
            }
            else
                BidDirection = RateChangeDirections.Flat;

            if (tick.HasAsk)
            {
                AskDirection = GetDirection(Ask, tick.Ask);
                Ask = tick.Ask;
                NotifyOfPropertyChange("Ask");
            }
            else
                AskDirection = RateChangeDirections.Flat;

            NotifyOfPropertyChange("BidDirection");
            NotifyOfPropertyChange("AskDirection");
        }

        private static RateChangeDirections GetDirection(double? oldVal, double newVal)
        {
            if (oldVal == null || oldVal.Value == newVal)
                return RateChangeDirections.Flat;
            else if (oldVal.Value < newVal)
                return RateChangeDirections.Up;
            return RateChangeDirections.Down;
        }

        public void Close()
        {
            this.model.Unsubscribe(this);
        }
    }

    public enum RateChangeDirections
    {
        Flat,
        Up,
        Down
    }
}