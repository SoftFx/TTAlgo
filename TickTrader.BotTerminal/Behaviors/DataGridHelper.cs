using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace TickTrader.BotTerminal
{
    public static class DataGridHelper
    {
        public static readonly DependencyProperty FocusRowProperty = DependencyProperty.RegisterAttached("FocusRow", typeof(int),
            typeof(DataGridHelper), new PropertyMetadata(new PropertyChangedCallback(FocusRowPropertyChanged)));

        public static void SetFocusRow(DependencyObject element, int value)
        {
            element.SetValue(FocusRowProperty, value);
        }

        public static int GetFocusRow(DependencyObject element)
        {
            return (int)element.GetValue(FocusRowProperty);
        }

        private static void FocusRowPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d == null || !(d is DataGrid))
                return;

            var grid = d as DataGrid;

            var selectRow = grid.SelectedItem;

            if (selectRow == null)
                return;

            grid.UpdateLayout();
            grid.ScrollIntoView(selectRow);
        }
    }
}
