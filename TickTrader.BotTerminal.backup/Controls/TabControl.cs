using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace TickTrader.BotTerminal
{
    public class TabControl : System.Windows.Controls.TabControl
    {
        public static DependencyProperty TabHeaderPaddingProperty =
            DependencyProperty.Register("TabHeaderPadding", typeof(Thickness), typeof(TabControl));

        public Thickness TabHeaderPadding
        {
            get { return (Thickness)GetValue(TabHeaderPaddingProperty); }
            set { SetValue(TabHeaderPaddingProperty, value); }
        }
    }
}
