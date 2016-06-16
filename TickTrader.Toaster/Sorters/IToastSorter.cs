using System.Windows;

namespace TickTrader.Toaster.Sorters
{
    public interface IToastSorter
    {
        Point GetPosition(IToastSize toast, int postion);
    }
}
