using System;
using System.Windows;

namespace TickTrader.Toaster.Animation
{
    public interface IAnimation: IDisposable
    {
        event Action Completed;
        TimeSpan Duration { get; set; }
        void Start(FrameworkElement element);
        void Stop();
    }
}
