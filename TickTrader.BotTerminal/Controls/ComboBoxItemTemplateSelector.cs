using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace TickTrader.BotTerminal
{
    internal class ComboBoxItemTemplateSelector: DataTemplateSelector
    {
        public DataTemplate SelectedItemTemplate { get; set; }
        public DataTemplate ItemTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            bool selected = true;
            
            FrameworkElement fe = container as FrameworkElement;
            if (fe != null)
            {
                DependencyObject parent = fe.TemplatedParent;
                if (parent != null)
                {
                    ComboBoxItem cbi = parent as ComboBoxItem;
                    if (cbi != null)
                        selected = false;
                }
            }

            if (selected)
                return SelectedItemTemplate;
            else
                return ItemTemplate;
        }
    }
}
