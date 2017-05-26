using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Xceed.Wpf.AvalonDock.Layout;

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

    public class AvalonAnchorTemplateSelector : DataTemplateSelector
    {
        public DataTemplate AnchorTemplate { get; set; }

        public DataTemplate TradesTemplate { get; set; }

        public DataTemplate HistoryTemplate { get; set; }


        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var a = item as LayoutAnchorable;

            if (a != null)
            {
                if (a.Title == ResxCore.Instance["Tab_Trade"] as string)
                {
                    return TradesTemplate;
                }
                if (a.Title == ResxCore.Instance["Tab_History"] as string)
                {
                    return HistoryTemplate;
                }
            }

            return AnchorTemplate;
        }
    }
}
