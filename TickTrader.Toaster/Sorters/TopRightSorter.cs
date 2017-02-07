using System.Windows;
using System.Windows.Forms;


namespace TickTrader.Toaster.Sorters
{
    public class TopRightSorter : IToastSorter
    {
        public Point GetPosition(IToastSize toast, int postion)
        {
            var mainWindow = System.Windows.Application.Current.MainWindow;
            var appWindowBounds = new System.Drawing.Rectangle((int)mainWindow.Left, (int)mainWindow.Top, (int)mainWindow.ActualWidth, (int)mainWindow.ActualHeight);
            var screenBounds = Screen.FromRectangle(appWindowBounds).Bounds;
            return new Point(screenBounds.Left + screenBounds.Width - toast.ActualWidth, screenBounds.Top + toast.ActualHeight * (postion));
        }
    }
}
