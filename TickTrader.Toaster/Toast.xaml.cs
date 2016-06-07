using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace TickTrader.Toaster
{
    public partial class ToastWindow : Window, IToast
    {
        Storyboard fadeOutAnimation;
        Storyboard moveAnimation;
        Action<IToast> afterCloseAction;
        public ToastWindow(object message, TimeSpan lifeTime, Point startingPosition)
        {
            InitializeComponent();
            ContentHolder.Content = message;
            Topmost = true;
            ShowInTaskbar = false;
            ShowActivated = false;
            Owner = Application.Current.MainWindow;
            Top = startingPosition.Y;
            Left = startingPosition.X;
            Loaded += ToastWindowLoaded;
            Closed += ToastWindowClosed;
            MouseLeftButtonDown += ToastWindowMouseLeftButtonDown;

            moveAnimation = new Storyboard();
            fadeOutAnimation = ToastHelper.GetFadeOutAnimation(lifeTime, ref ToasterInstance);
            fadeOutAnimation.Completed += AnimationCompleted;
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
            if (moveAnimation != null)
            {
                moveAnimation.Stop(this);
                moveAnimation.Remove(this);
            }

            if (!animate)
            {
                Top = position.Y;
                Left = position.X;
            }
            else
            {
                var animationY = new DoubleAnimation(Top, position.Y, TimeSpan.FromSeconds(0.2));
                var animationX = new DoubleAnimation(Left, position.X, TimeSpan.FromSeconds(0.2));

                Storyboard.SetTargetProperty(animationY, new PropertyPath(Window.TopProperty));
                Storyboard.SetTargetProperty(animationX, new PropertyPath(Window.LeftProperty));

                moveAnimation.Children.Add(animationY);
                moveAnimation.Children.Add(animationX);

                moveAnimation.Begin(this, true);
            }
        }

        void IToast.AfterClose(Action<IToast> func)
        {
            afterCloseAction = func;
        }
    }

    internal interface IToast
    {
        void Show();
        void Move(Point position, bool animate);
        void AfterClose(Action<IToast> func);
        double ActualHeight { get; }
        double ActualWidth { get; }

    }
}
