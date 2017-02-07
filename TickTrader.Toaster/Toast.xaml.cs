using System;
using System.Windows;
using System.Windows.Input;
using TickTrader.Toaster.Animation;

namespace TickTrader.Toaster
{
    public interface IToastSize
    {
        double ActualHeight { get; }
        double ActualWidth { get; }
    }
    public interface IToast : IToastSize
    {
        IMoveAnimation MoveAnimation { get; }
        ICloseAnimation CloseAnimation { get; }
        void Show();
        void Move(Point position, bool animate);
    }


    public sealed partial class Toast : Window, IToast
    {
        public IMoveAnimation MoveAnimation { get; private set; }
        public ICloseAnimation CloseAnimation { get; private set; }
        public object Message
        {
            get { return (object)GetValue(MessageProperty); }
            set { SetValue(MessageProperty, value); }
        }

        public static readonly DependencyProperty MessageProperty =
            DependencyProperty.Register(nameof(Message), typeof(object), typeof(Toast));

        internal Toast(object message, double duration, DirectMovementAnimation directMovementAnimation, FadeOutAnimation fadeOutAnimation)
        {
            InitializeComponent();

            Owner = Application.Current.MainWindow;
            Closed += ToastClosed;
            MouseLeftButtonDown += (s, e) => Close();

            MoveAnimation = directMovementAnimation;
            CloseAnimation = fadeOutAnimation;
            CloseAnimation.Completed += () => Close();

            Loaded += (s, e) => CloseAnimation.Start(ToasterInstance);

            Message = message;
        }

        public new void Show()
        {
            base.Show();
            var position = ToastSorter.Sorter.GetPosition(this, 0);
            Left = position.X;
            Top = position.Y;
        }

        void IToast.Move(Point position, bool animate)
        {
            MoveAnimation?.Stop();

            if (MoveAnimation == null || !animate)
            {
                Left = position.X;
                Top = position.Y;
            }
            else
            {
                MoveAnimation.From = new Point(Left, Top);
                MoveAnimation.To = position;
                MoveAnimation.Start(this);
            }
        }

        private void ToastMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Close();
        }

        private void ToastClosed(object sender, EventArgs e)
        {
            ToastSorter.AdjustWindows(true);
            MoveAnimation.Dispose();
            CloseAnimation.Dispose();
        }


        public static void Pop(object message, double duration = 10000)
        {
            Application.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background,
                    new Action(() =>
                    {
                        IToast toast = new Toast(message, duration,
                            new DirectMovementAnimation() { Duration = TimeSpan.FromMilliseconds(200) },
                            new FadeOutAnimation() { Duration = TimeSpan.FromMilliseconds(duration) });
                        toast.Show();
                        ToastSorter.AdjustWindows(true);
                    }));
        }
    }
}
