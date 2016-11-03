using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace TickTrader.BotTerminal
{
    public class BindingProxy : Freezable
    {
        public object Data
        {
            get { return ( object)GetValue(DataProperty); }
            set { SetValue(DataProperty, value); }
        }

        public static readonly DependencyProperty DataProperty =
            DependencyProperty.Register(nameof(Data), typeof( object), typeof(BindingProxy));

        protected override Freezable CreateInstanceCore()
        {
            return new BindingProxy();
        }
    }
}
