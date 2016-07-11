using System;
using System.Linq;
using System.Windows;
using TickTrader.Toaster.Sorters;

namespace TickTrader.Toaster
{
    public class ToastSorter
    {
        private static IToastSorter sorter;
        public static IToastSorter Sorter
        {
            get
            {
                if (sorter == null)
                    sorter = new TopRightSorter();
                return sorter;
            }
            set { sorter = value; }
        }

        public static void AdjustWindows(bool useAnimation)
        {
            var toasts = Application.Current.Windows.OfType<IToast>();
            var count = toasts.Count();

            foreach (IToast toast in toasts)
                Application.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.ApplicationIdle,
                    new Action(() => toast.Move(Sorter.GetPosition(toast, --count), useAnimation)));
        }
    }
}
