using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;

namespace TickTrader.BotTerminal
{
    public class AutoColumnsGrid : UniformGrid
    {
        public static readonly DependencyProperty ColumnMinWidthProperty =
            DependencyProperty.Register(nameof(MinColumnWidth), typeof(double), typeof(AutoColumnsGrid), new FrameworkPropertyMetadata(40.0));

        public static readonly DependencyProperty MaxColumnsProperty =
            DependencyProperty.Register(nameof(MaxColumns), typeof(int), typeof(AutoColumnsGrid), new FrameworkPropertyMetadata(0));

        public static readonly DependencyProperty MinColumnsProperty =
            DependencyProperty.Register(nameof(MinColumns), typeof(int), typeof(AutoColumnsGrid), new FrameworkPropertyMetadata(0));

        public double MinColumnWidth
        {
            get { return (double)GetValue(ColumnMinWidthProperty); }
            set { SetValue(ColumnMinWidthProperty, value); }
        }

        public int MaxColumns
        {
            get { return (int)GetValue(MaxColumnsProperty); }
            set { SetValue(MaxColumnsProperty, value); }
        }

        public int MinColumns
        {
            get { return (int)GetValue(MinColumnsProperty); }
            set { SetValue(MinColumnsProperty, value); }
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            if (sizeInfo.WidthChanged)
                RecalculateColumnsCount(sizeInfo.NewSize);

            base.OnRenderSizeChanged(sizeInfo);
        }

        protected override void OnVisualChildrenChanged(DependencyObject visualAdded, DependencyObject visualRemoved)
        {
            RecalculateColumnsCount(RenderSize);

            base.OnVisualChildrenChanged(visualAdded, visualRemoved);
        }

        private void RecalculateColumnsCount(Size areaSize)
        {
            var columnMinSize = MinColumnWidth;

            var minCount = MinColumns > 0 ? MinColumns : 1;

            if (columnMinSize > 0)
            {
                var newColumsCount = (int)Math.Floor(areaSize.Width / columnMinSize);

                if (MaxColumns > 0 && newColumsCount > MaxColumns)
                    newColumsCount = MaxColumns;

                if (newColumsCount <= minCount)
                    newColumsCount = minCount;

                if (newColumsCount > VisualChildrenCount)
                    newColumsCount = VisualChildrenCount;

                if (newColumsCount != Columns)
                    Columns = newColumsCount;
            }
        }
    }
}
