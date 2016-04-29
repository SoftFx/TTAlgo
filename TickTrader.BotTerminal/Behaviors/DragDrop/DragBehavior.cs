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
        private FrameworkElement _dragScope;
        private IDragAdorner _dragAdorner;

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

        public DragBehavior()
        {
            _dragScope = Application.Current.MainWindow.Content as FrameworkElement;
        }

        protected override void OnAttached()
        {
            base.OnAttached();

            AssociatedObject.MouseLeave += AssociatedObject_MouseLeave;
            AssociatedObject.QueryContinueDrag += AssociatedObject_QueryContinueDrag;
            AssociatedObject.GiveFeedback += AssociatedObject_GiveFeedback;
        }
        protected override void OnDetaching()
        {
            base.OnDetaching();

            AssociatedObject.QueryContinueDrag -= AssociatedObject_QueryContinueDrag;
            AssociatedObject.MouseLeave -= AssociatedObject_MouseLeave;
            AssociatedObject.GiveFeedback -= AssociatedObject_GiveFeedback;
        }

        private void AssociatedObject_GiveFeedback(object sender, GiveFeedbackEventArgs e)
        {
            e.UseDefaultCursors = true;

            if (e.Effects == DragDropEffects.None)
                _dragAdorner.DropState = DropState.CannotDrop;
            else
                _dragAdorner.DropState = DropState.CanDrop;

            e.Handled = true;
        }
        
        private void AssociatedObject_QueryContinueDrag(object sender, QueryContinueDragEventArgs e)
        {
            _dragAdorner.Position = _dragScope.PointFromScreen(WinApiHelper.GetMousePosition());
        }

        private void AssociatedObject_MouseLeave(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && !_isDragging)
                StarDrag(e);
        }

        private void StarDrag(MouseEventArgs e)
        {
            _isDragging = true;

            object data;
            if ((data = Data ?? AssociatedObject.DataContext) != null)
            {
                using (_dragAdorner = new DragAdorner(_dragScope, e.OriginalSource as FrameworkElement))
                {
                    var result = DragDrop.DoDragDrop(AssociatedObject, AssociatedObject.DataContext, DragDropEffects.Move);
                }
                _dragAdorner = null;
            }

            _isDragging = false;
        }
    }
}
