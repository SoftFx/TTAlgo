using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace TickTrader.BotTerminal
{
    internal class KeyValueLabel : Control
    {
        public static readonly DependencyProperty KeyProperty =
           DependencyProperty.Register(nameof(Key), typeof(object), typeof(KeyValueLabel));

        public static readonly DependencyProperty ValueProperty =
           DependencyProperty.Register(nameof(Value), typeof(object), typeof(KeyValueLabel),
               new FrameworkPropertyMetadata() { BindsTwoWayByDefault = true });


        public object Key
        {
            get { return GetValue(KeyProperty); }
            set { SetValue(KeyProperty, value); }
        }

        public object Value
        {
            get { return GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }
    }
}
