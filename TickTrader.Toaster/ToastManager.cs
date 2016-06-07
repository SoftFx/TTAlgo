using System;
using System.Collections.ObjectModel;

namespace TickTrader.Toaster
{
    public enum ToastPosition
    {
        TopRight,
        TopLeft,
        BottomRight,
        BottomLeft
    }

    public class ToastManager
    {
        private static volatile ToastManager instance;
        private static object syncObj = new object();
        private IToastSorter lookingForOrder;
        private ObservableCollection<IToast> toasts;
        private int First = 0;

        private ToastManager()
        {
            toasts = new ObservableCollection<IToast>();
            toasts.CollectionChanged += ToastCollectionChanged;
            lookingForOrder = ToastHelper.GetSorter(ToastPosition.TopRight);
        }

        private void ToastCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                var toast = (IToast)e.NewItems[0];
                toast.Show();
                lookingForOrder.SetPosition(toast, First, false);
                AdjustWindows(true);
            }
            else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
            {
                AdjustWindows(true);
            }
        }

        public static ToastManager Instance
        {
            get
            {
                if (instance == null)
                    lock (syncObj)
                    {
                        if (instance == null)
                            instance = new ToastManager();
                    }

                return instance;
            }
        }

        public void Pop(object message, double duration = 8000)
        {
            System.Windows.Application.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.ApplicationIdle,
                    new Action(() =>
                    {
                        IToast toast = new ToastWindow(message, duration);
                        toast.AfterClose(RemoveToast);
                        toasts.Add(toast);
                    }));
        }

        private void AdjustWindows(bool useAnimation)
        {
            var count = toasts.Count;

            foreach (IToast toast in toasts)
                System.Windows.Application.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.ApplicationIdle,
                    new Action(() => lookingForOrder.SetPosition(toast, --count, useAnimation)));
        }

        private void RemoveToast(IToast toast)
        {
            toasts.Remove(toast);
        }
    }
}
