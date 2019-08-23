using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace TickTrader.BotTerminal
{
    public class Icon : DependencyObject
    {
        public static DependencyProperty FillAttachedProperty = DependencyProperty.RegisterAttached("Fill", typeof(Brush), typeof(Icon),
            new FrameworkPropertyMetadata(null,
                FrameworkPropertyMetadataOptions.Inherits
                | FrameworkPropertyMetadataOptions.AffectsRender));

        public static void SetFill(UIElement element, object value)
        {
            element.SetValue(FillAttachedProperty, value);
        }

        public static object GetFill(UIElement element)
        {
            return element.GetValue(FillAttachedProperty);
        }
    }
}
