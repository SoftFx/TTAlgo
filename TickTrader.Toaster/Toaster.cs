using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;

namespace TickTrader.Toaster
{
    public enum ToastPosition
    {
        TopRight,
        TopLeft,
        BottomRight,
        BottomLeft
    }

    public class Toaster
    {
        private static IToastSorter lookingForOrder;
        private static Action<IToast> adjustWindowsAction;

        static Toaster()
        {
            lookingForOrder = ToastHelper.GetSorter(ToastPosition.TopRight);
            adjustWindowsAction = new Action<IToast>(x => AdjustWindows(true));
        }

        public static void Pop(object message, double duration = 8000)
        {
            Application.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.ApplicationIdle,
                    new Action(() =>
                    {
                        IToast toast = new ToastWindow(message, duration);
                        toast.AfterClose(adjustWindowsAction);
                        toast.Show();
                        lookingForOrder.SetPosition(toast, 0, false);
                        AdjustWindows(true);
                    }));
        }

        private static void AdjustWindows(bool useAnimation)
        {
            var toasts = Application.Current.Windows.OfType<IToast>();
            var count = toasts.Count();

            foreach (IToast toast in toasts)
                Application.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.ApplicationIdle,
                    new Action(() => lookingForOrder.SetPosition(toast, --count, useAnimation)));
        }
    }
}
