using System.Windows;
using System.Windows.Forms;

namespace TickTrader.Toaster.Sorters
{
    public class TopLeftSorter : IToastSorter
    {
        public Point GetPosition(IToastSize toast, int postion)
        {
            var mainWindow = System.Windows.Application.Current.MainWindow;
            var appWindowBounds = new System.Drawing.Rectangle((int)mainWindow.Left, (int)mainWindow.Top, (int)mainWindow.ActualWidth, (int)mainWindow.ActualHeight);
            var screenBounds = Screen.FromRectangle(appWindowBounds).Bounds;
            return new Point(screenBounds.Left, screenBounds.Top + toast.ActualHeight * (postion));
        }
    }
}
