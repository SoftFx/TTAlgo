using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace TickTrader.BotTerminal
{
    public class AvalonDockTemplateSelector : DataTemplateSelector
    {
        public DataTemplate DocumentTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is ContentPresenter || item is ContentControl)
                return null;

            return DocumentTemplate;
        }
    }

    public class AvalonDockStyleSelector : StyleSelector
    {
        public Style DocumentStyle { get; set; }

        public override Style SelectStyle(object item, DependencyObject container)
        {
            if (item == null)
                return null;

            if (item is ContentPresenter || item is ContentControl)
                return null;

            return DocumentStyle;
        }
    }
}
