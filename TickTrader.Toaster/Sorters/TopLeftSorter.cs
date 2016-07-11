using System.Windows;
using System.Windows.Forms;

namespace TickTrader.Toaster.Sorters
{
    public class TopLeftSorter : IToastSorter
    {
        public Point GetPosition(IToastSize toast, int postion)
        {
            return new Point(0, toast.ActualHeight * (postion));
        }
    }
}
