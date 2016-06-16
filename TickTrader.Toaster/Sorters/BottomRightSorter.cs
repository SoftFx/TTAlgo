using System.Windows;
using System.Windows.Forms;

namespace TickTrader.Toaster.Sorters
{
    public class BottomRightSorter : IToastSorter
    {
        public Point GetPosition(IToastSize toast, int postion)
        {
            var workingArea = Screen.PrimaryScreen.WorkingArea;
            return new Point(workingArea.Width - toast.ActualWidth, workingArea.Height - toast.ActualHeight * (postion+1));
        }
    }
}
