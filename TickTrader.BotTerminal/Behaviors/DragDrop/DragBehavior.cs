using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interactivity;

namespace TickTrader.BotTerminal
{
    internal class DragBehavior : Behavior<FrameworkElement>
    {
        private bool _isDragging;
        private Window _dragWindow;
        private Point _startMousePosition;
        private DragSource _dragSource;

        /// <summary>
        /// Represents the data that will be dragged. Default is taken from AssociatedObject.DataContext
        /// </summary>
        public object Data
        {
            get { return (object)GetValue(MyPropertyProperty); }
            set { SetValue(MyPropertyProperty, value); }
        }
        public static readonly DependencyProperty MyPropertyProperty =
            DependencyProperty.Register(nameof(Data), typeof(object), typeof(DragBehavior), new UIPropertyMetadata(null));

        protected override void OnAttached()
        {
            base.OnAttached();

            AssociatedObject.MouseLeftButtonDown += AssociatedObjectMouseLeftButtonDown;
            AssociatedObject.MouseMove += AssociatedObjectMouseMove;
            AssociatedObject.QueryContinueDrag += AssociatedObjectQueryContinueDrag;
            AssociatedObject.GiveFeedback += AssociatedObjectGiveFeedback;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            AssociatedObject.MouseLeftButtonDown -= AssociatedObjectMouseLeftButtonDown;
            AssociatedObject.MouseMove -= AssociatedObjectMouseMove;
            AssociatedObject.QueryContinueDrag -= AssociatedObjectQueryContinueDrag;
            AssociatedObject.GiveFeedback -= AssociatedObjectGiveFeedback;
        }

        private void AssociatedObjectMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && !_isDragging)
                StarDrag(e);
        }

        private void AssociatedObjectMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _startMousePosition = e.GetPosition(AssociatedObject);
        }

        private void AssociatedObjectGiveFeedback(object sender, GiveFeedbackEventArgs e)
        {
            if (e.Effects == DragDropEffects.None)
            {
                Mouse.OverrideCursor = Cursors.No;
                _dragSource.DropState = DropState.CannotDrop;
            }
            else
            {
                Mouse.OverrideCursor = Cursors.Arrow;
                _dragSource.DropState = DropState.CanDrop;
            }

            e.Handled = true;
        }

        private void AssociatedObjectQueryContinueDrag(object sender, QueryContinueDragEventArgs e)
        {
            SetDragWindowPosition(PlatformProxy.GetMousePosition());
        }

        private void StarDrag(MouseEventArgs e)
        {
            _isDragging = true;

            object data;
            if ((data = Data ?? AssociatedObject.DataContext) != null)
            {
                _dragSource = new DragSource(AssociatedObject, data);

                ShowDragDrop();

                var result = DragDrop.DoDragDrop(AssociatedObject, data, DragDropEffects.Move);

                Mouse.OverrideCursor = null;
                _dragSource = null;
                _dragWindow.Close();
                _dragWindow = null;
            }

            _isDragging = false;
        }

        private void ShowDragDrop()
        {
            _dragWindow = new TransparentContainer(_dragSource);
            SetDragWindowPosition(PlatformProxy.GetMousePosition());
            _dragWindow.Show();
        }

        private void SetDragWindowPosition(Point point)
        {
            _dragWindow.Left = point.X - _startMousePosition.X;
            _dragWindow.Top = point.Y - _startMousePosition.Y;
        }
    }
}
