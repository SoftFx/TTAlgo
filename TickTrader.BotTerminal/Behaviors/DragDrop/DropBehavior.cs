using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interactivity;

namespace TickTrader.BotTerminal
{
    public class DropBehavior : Behavior<FrameworkElement>
    {
        private Type _droppedDataType;

        protected override void OnAttached()
        {
            base.OnAttached();

            this.AssociatedObject.AllowDrop = true;
            this.AssociatedObject.Drop += new DragEventHandler(AssociatedObject_Drop);
        }
        
        private void AssociatedObject_Drop(object sender, DragEventArgs e)
        {
            var dropHandler = AssociatedObject.DataContext as IDropHandler;
            if (CanHandleDrop(dropHandler, e.Data, out _droppedDataType))
                dropHandler.Drop(e.Data.GetData(_droppedDataType));

            e.Handled = true;
        }

        private bool CanHandleDrop(IDropHandler dropHandler, IDataObject data, out Type droppedDataType)
        {
            droppedDataType = dropHandler?.AcceptedTypes?.FirstOrDefault(t => data.GetDataPresent(t));
            return droppedDataType != null;
        }
    }
}
