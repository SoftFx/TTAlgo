using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace TickTrader.BotTerminal
{
    internal class DialogPageControl : TabControl
    {
        public static DependencyProperty HeaderWidthProperty  = DependencyProperty.Register(
            "HeaderWidth",
            typeof(double),
            typeof(DialogPageControl));

        public double HeaderWidth
        {
            get { return (double)GetValue(HeaderWidthProperty); }
            set { SetValue(HeaderWidthProperty, value); }
        }
    }
}
