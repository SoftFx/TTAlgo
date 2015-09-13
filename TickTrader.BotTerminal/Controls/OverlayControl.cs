using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace TickTrader.BotTerminal
{
    public class OverlayControl : DependencyObject
    {
        public static DependencyProperty ContentAttachedProperty = DependencyProperty.RegisterAttached("Content", typeof(UIElement), typeof(OverlayControl));

        public static void SetContent(UIElement element, UIElement value)
        {
            element.SetValue(ContentAttachedProperty, value);
        }

        public static UIElement GetContent(UIElement element)
        {
            return (UIElement)element.GetValue(ContentAttachedProperty);
        }
    }
}
