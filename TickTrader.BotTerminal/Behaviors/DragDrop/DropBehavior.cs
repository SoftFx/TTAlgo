using System.Windows;
using System.Windows.Interactivity;

namespace TickTrader.BotTerminal
{
    internal sealed class DropBehavior : Behavior<FrameworkElement>
    {
        private bool _canHandleDrop;

        protected override void OnAttached()
        {
            base.OnAttached();

            this.AssociatedObject.AllowDrop = true;
            this.AssociatedObject.Drop += new DragEventHandler(AssociatedObject_Drop);
            this.AssociatedObject.DragEnter += AssociatedObject_DragEnter;
            AssociatedObject.DragOver += AssociatedObject_DragOver;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            this.AssociatedObject.AllowDrop = false;
            this.AssociatedObject.Drop -= new DragEventHandler(AssociatedObject_Drop);
            this.AssociatedObject.DragEnter -= AssociatedObject_DragEnter;
            AssociatedObject.DragOver -= AssociatedObject_DragOver;
        }

        private void AssociatedObject_DragOver(object sender, DragEventArgs e)
        {
            UpdateDragDropEffect(e);
            e.Handled = true;
        }

        private void AssociatedObject_DragEnter(object sender, DragEventArgs e)
        {
            _canHandleDrop = CanHandleDrop(e.Data);
            UpdateDragDropEffect(e);
            e.Handled = true;
        }

        private void UpdateDragDropEffect(DragEventArgs e)
        {
            e.Effects = _canHandleDrop ? DragDropEffects.Move : DragDropEffects.None;
        }

        private void AssociatedObject_Drop(object sender, DragEventArgs e)
        {
            (AssociatedObject.DataContext as IDropHandler)?.Drop(e.Data.GetData(e.Data.GetFormats()[0]));
            e.Handled = true;
        }

        private bool CanHandleDrop(IDataObject data)
        {
            var canHandle = (AssociatedObject.DataContext as IDropHandler)?.CanDrop(data.GetData(data.GetFormats()[0]));
            return canHandle.HasValue && canHandle.Value;
        }
    }
}
