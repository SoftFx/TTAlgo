using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace TickTrader.BotTerminal
{
    class SplitterPanel : NoParentPanel
    {
        private double newElementRatio = 1;
        private Grid rootGrid;

        public SplitterPanel()
        {
        }

        public double NewElementSizeRatio
        {
            get { return newElementRatio; }
            set
            {
                if (value <= 0)
                    throw new InvalidOperationException("Value of property NewElementSizeRatio must be greater than zero.");
                this.newElementRatio = value;
            }
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            EnsureGrid();

            rootGrid.Measure(availableSize);
            return rootGrid.DesiredSize;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            EnsureGrid();

            rootGrid.Arrange(new Rect(new Point(0, 0), finalSize));
            return rootGrid.RenderSize;
        }

        private void EnsureGrid()
        {
            if (rootGrid == null)
            {
                this.rootGrid = new Grid();
                this.AddVisualChild(rootGrid);
            }
        }

        protected override void OnChildAdded(UIElement child, int index)
        {
            EnsureGrid();

            if (index < 0)
                throw new Exception("Invalid operation: child added at " + index + " index");

            rootGrid.BeginInit();

            if (index == 0)
            {
                InsertElement(0, child);
                if (rootGrid.Children.Count > 1)
                    InsertSplitter(1);
                UpdateRowNumber(0);
                UpdateHeights(0);
            }
            else
            {
                int inserGridIndex = index * 2 - 1;
                InsertElement(inserGridIndex, child);
                InsertSplitter(inserGridIndex);
                UpdateRowNumber(inserGridIndex);
                UpdateHeights(inserGridIndex + 1);
            }

            rootGrid.EndInit();
        }

        protected override void OnChildRemoved(UIElement child, int index)
        {
            if (index < 0)
                throw new Exception("Invalid operation: child removed at " + index + " index");

            if (index == 0)
            {
                RemoveFromGrid(0);
                if (rootGrid.Children.Count > 0)
                    RemoveFromGrid(0);
                UpdateRowNumber(0);
            }
            else
            {
                int removeGridIndex = index * 2 - 1;
                RemoveFromGrid(removeGridIndex);
                RemoveFromGrid(removeGridIndex);
                UpdateRowNumber(removeGridIndex);
            }
        }

        private void UpdateHeights(int insertedIndex)
        {
            rootGrid.RowDefinitions[insertedIndex].Height = new GridLength(newElementRatio, GridUnitType.Star);

            if (Children.Count > 1)
            {
                double oldTotal = 0;

                for (int i = 0; i < rootGrid.RowDefinitions.Count; i++)
                {
                    var row = rootGrid.RowDefinitions[i];
                    if (!row.Height.IsAuto && i != insertedIndex)
                        oldTotal += row.Height.Value;
                }

                double k = (1 + (Children.Count - 2) * newElementRatio) / oldTotal;

                for (int i = 0; i < rootGrid.RowDefinitions.Count; i++)
                {
                    var row = rootGrid.RowDefinitions[i];
                    if (!row.Height.IsAuto && i != insertedIndex)
                    {
                        double newHeight = row.Height.Value * k;
                        row.Height = new GridLength(newHeight, GridUnitType.Star);
                    }
                }
            }
        }

        private void InsertElement(int index, UIElement element)
        {
            rootGrid.RowDefinitions.Insert(index, new RowDefinition() { Height = new GridLength(0, GridUnitType.Star) });
            rootGrid.Children.Insert(index, element);
        }

        private void InsertSplitter(int index)
        {
            rootGrid.Children.Insert(index, new GridSplitter()
            {
                Height = 4,
                Background = Brushes.Transparent,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Stretch
            });

            rootGrid.RowDefinitions.Insert(index, new RowDefinition() { Height = GridLength.Auto });
        }

        private void RemoveFromGrid(int index)
        {
            rootGrid.RowDefinitions.RemoveAt(index);
            rootGrid.Children.RemoveAt(index);
        }

        private void UpdateRowNumber(int from)
        {
            for (int i = from; i < rootGrid.Children.Count; i++)
                Grid.SetRow(rootGrid.Children[i], i);
        }
    }
}
