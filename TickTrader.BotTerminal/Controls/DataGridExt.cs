using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace TickTrader.BotTerminal
{
    public static class DataGridExt
    {
        public static readonly DependencyProperty AutoSizeProperty = DependencyProperty.RegisterAttached("AutoSize",
            typeof(bool), typeof(DataGridExt), new UIPropertyMetadata(false, AutoSizeChanged));

        public static bool GetAutoSize(DependencyObject obj)
        {
            return (bool)obj.GetValue(AutoSizeProperty);
        }

        public static void SetAutoSize(DependencyObject obj, bool value)
        {
            obj.SetValue(AutoSizeProperty, value);
        }

        private static void AutoSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var dataGrid = d as DataGrid;

            if (dataGrid == null)
                throw new InvalidOperationException("AutoSize property may only be set on DataGrid control!");

            var autoSize = (bool)e.NewValue;

            if (autoSize)
            {
                dataGrid.Columns.CollectionChanged += Columns_CollectionChanged;
                ApplyAutoSize(dataGrid);
                dataGrid.HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden;
            }
            else
            {
                dataGrid.Columns.CollectionChanged -= Columns_CollectionChanged;
                ApplyFixedSize(dataGrid);
                dataGrid.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
            }
        }

        private static void Columns_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (var item in e.NewItems)
                    ApplyAutoSize((DataGridColumn)item);
            }
        }

        private static void ApplyAutoSize(DataGrid grid)
        {
            foreach (var column in grid.Columns)
                ApplyAutoSize(column);
        }

        private static void ApplyAutoSize(DataGridColumn column)
        {
            if (column.Width.IsAbsolute)
                column.Width = new DataGridLength(column.Width.Value, DataGridLengthUnitType.Star);
        }

        private static void ApplyFixedSize(DataGrid grid)
        {
            foreach (var column in grid.Columns)
                ApplyFixedSize(column);
        }

        private static void ApplyFixedSize(DataGridColumn column)
        {
            if (column.Width.IsStar)
                column.Width = new DataGridLength(column.Width.Value, DataGridLengthUnitType.Pixel);
        }
    }
}
