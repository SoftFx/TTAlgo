using System.Windows;
using System.Windows.Forms;

namespace TickTrader.Toaster.Sorters
{
    public class BottomLeftSorter : IToastSorter
    {
        public Point GetPosition(IToastSize toast, int postion)
        {
            var workingArea = Screen.PrimaryScreen.WorkingArea;
            return new Point(0, workingArea.Height - toast.ActualHeight * (postion+1));
        }
    }
}
