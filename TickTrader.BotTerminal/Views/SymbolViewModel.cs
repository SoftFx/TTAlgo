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

        public int Depth { get; private set; }

        public event System.Action DepthChanged = delegate { };

        public void OnRateUpdate(Quote tick)
        {
            Bid = tick.Bid;
            Ask = tick.Ask;
            NotifyOfPropertyChange("Bid");
            NotifyOfPropertyChange("Ask");
        }

        public void Close()
        {
            this.model.Unsubscribe(this);
        }
    }
}