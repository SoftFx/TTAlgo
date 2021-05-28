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
        public static DependencyProperty ContentAttachedProperty = DependencyProperty.RegisterAttached("Content", typeof(object), typeof(OverlayControl));

        public static void SetContent(UIElement element, object value)
        {
            element.SetValue(ContentAttachedProperty, value);
        }

        public static object GetContent(UIElement element)
        {
            return element.GetValue(ContentAttachedProperty);
        }

        public static DependencyProperty ContentTemplateAttachedProperty = DependencyProperty.RegisterAttached("ContentTemplate", typeof(DataTemplate), typeof(OverlayControl));

        public static void SetContentTemplate(UIElement element, DataTemplate value)
        {
            element.SetValue(ContentAttachedProperty, value);
        }

        public static DataTemplate GetContentTemplate(UIElement element)
        {
            return (DataTemplate)element.GetValue(ContentAttachedProperty);
        }

    }
}
