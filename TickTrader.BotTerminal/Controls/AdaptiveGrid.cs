using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace TickTrader.BotTerminal
{
    public class AdaptiveGrid : Panel
    {
        public static readonly DependencyProperty ColumnMinWidthProperty =
            DependencyProperty.Register(nameof(MinColumnWidth), typeof(double), typeof(AdaptiveGrid), new FrameworkPropertyMetadata(40.0));

        public static readonly DependencyProperty MaxColumnsProperty =
            DependencyProperty.Register(nameof(MaxColumns), typeof(int), typeof(AdaptiveGrid), new FrameworkPropertyMetadata(0));

        public static readonly DependencyProperty MinColumnsProperty =
            DependencyProperty.Register(nameof(MinColumns), typeof(int), typeof(AdaptiveGrid), new FrameworkPropertyMetadata(0));

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

        protected override Size MeasureOverride(Size availableSize)
        {
            var columns = CalculateColumnsCount(availableSize);
            var columnsWidth = availableSize.Width / columns;

            int childNo = 0;
            var rowHeight = 0d;
            var rowY = 0d;
            //var cellX = 0d;
            foreach (UIElement child in Children)
            {
                var column = childNo % columns;

                if (column == 0)
                {
                    rowY += rowHeight;
                    rowHeight = 0;
                    //cellX = 0;
                }

                child.Measure(new Size(columnsWidth, double.PositiveInfinity));

                //cellX += columnsWidth;
                rowHeight = Math.Max(child.DesiredSize.Height, rowHeight);

                childNo++;
            }

            var totalHeight = rowY + rowHeight;

            if (VerticalAlignment == VerticalAlignment.Stretch && !double.IsInfinity(availableSize.Height))
            {
                var maxHeight = Math.Max(totalHeight, availableSize.Height);
                return new Size(availableSize.Width, maxHeight);
            }

            return new Size(availableSize.Width, totalHeight);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            var columns = CalculateColumnsCount(finalSize);
            var columnsWidth = finalSize.Width / columns;

            var rowY = 0d;
            var rowStart = 0;
            var rowCount = (int)Math.Ceiling((double)VisualChildrenCount / columns);

            for (int row = 0; row < rowCount; row++)
            {
                var rowHegith = 0d;

                for (int col = 0; col < columns && rowStart + col < VisualChildrenCount; col++)
                    rowHegith = Math.Max(rowHegith, Children[rowStart + col].DesiredSize.Height);

                var cellX = 0d;

                for (int col = 0; col < columns && rowStart + col < VisualChildrenCount; col++)
                {
                    var child = Children[rowStart + col];
                    child.Arrange(new Rect(new Point(cellX, rowY), new Size(columnsWidth, rowHegith)));
                    cellX += columnsWidth;
                }

                rowStart += columns;
                rowY += rowHegith;
            }

            var totalHeight = rowY;

            if (VerticalAlignment == VerticalAlignment.Stretch)
            {
                var maxHeight = Math.Max(totalHeight, finalSize.Height);
                return new Size(finalSize.Width, maxHeight);
            }

            return new Size(finalSize.Width, totalHeight);
        }

        private int CalculateColumnsCount(Size areaSize)
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

                return newColumsCount;
            }

            return 1;
        }
    }
}
