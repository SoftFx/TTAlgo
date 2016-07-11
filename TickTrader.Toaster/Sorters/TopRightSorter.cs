using System.Windows;
using System.Windows.Forms;


namespace TickTrader.Toaster.Sorters
{
    public class TopRightSorter : IToastSorter
    {
        public Point GetPosition(IToastSize toast, int postion)
        {
            var workingArea = Screen.PrimaryScreen.WorkingArea;
            return new Point(workingArea.Width - toast.ActualWidth, toast.ActualHeight * (postion));
        }
    }
}
