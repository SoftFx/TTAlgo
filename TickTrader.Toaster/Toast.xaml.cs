using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace TickTrader.Toaster
{
    internal interface IToast
    {
        void Show();
        void Move(Point position, bool animate);
        void AfterClose(Action<IToast> func);
        double ActualHeight { get; }
        double ActualWidth { get; }
    }

    internal partial class ToastWindow : Window, IToast
    {
        Storyboard fadeOutAnimation;
        MoveAnimation moveAnimation;
        Action<IToast> afterCloseAction;
        internal ToastWindow(object message, double duration)
        {
            InitializeComponent();
            Topmost = true;
            ShowInTaskbar = false;
            ShowActivated = false;
            Owner = Application.Current.MainWindow;

            Loaded += ToastWindowLoaded;
            Closed += ToastWindowClosed;
            MouseLeftButtonDown += ToastWindowMouseLeftButtonDown;

            moveAnimation = new MoveAnimation(this);
            fadeOutAnimation = ToastHelper.GetFadeOutAnimation(duration, ref ToasterInstance);
            fadeOutAnimation.Completed += AnimationCompleted;

            ContentHolder.Content = message;
        }

        private void ToastWindowMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            CloseWindow();
        }

        private void AnimationCompleted(object sender, EventArgs e)
        {
            CloseWindow();
        }

        private void ToastWindowLoaded(object sender, RoutedEventArgs e)
        {
            fadeOutAnimation.Begin(this);
        }

        private void ToastWindowClosed(object sender, EventArgs e)
        {
            try
            {
                afterCloseAction(this);
                afterCloseAction = null;
            }
            catch (Exception ex)
            {

            }
        }

        private void CloseWindow()
        {
            Close();

            fadeOutAnimation.Completed -= AnimationCompleted;
            Loaded -= ToastWindowLoaded;
            Closed -= ToastWindowClosed;
            MouseLeftButtonDown -= ToastWindowMouseLeftButtonDown;
        }


        void IToast.Move(Point position, bool animate)
        {
            moveAnimation.Stop();

            if (animate)
            {
                moveAnimation.Duration = animate ? TimeSpan.FromMilliseconds(200) : TimeSpan.FromSeconds(0);
                moveAnimation.From = new Point(Left, Top);
                moveAnimation.To = position;
                moveAnimation.Start();
            }
            else
            {
                Left = position.X;
                Top = position.Y;
            }
        }

        void IToast.AfterClose(Action<IToast> func)
        {
            afterCloseAction = func;
        }
    }


}
