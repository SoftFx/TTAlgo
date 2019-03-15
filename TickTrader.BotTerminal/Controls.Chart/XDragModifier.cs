using SciChart.Charting.ChartModifiers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace TickTrader.BotTerminal
{
    class XDragModifier : XAxisDragModifier
    {
        public override void OnAttached()
        {
            var marketViewport = ParentSurface.ViewportManager as ViewportManager;

            if (marketViewport != null)
                marketViewport.XDragModifier = this;

            base.OnAttached();
        }
    }
}
