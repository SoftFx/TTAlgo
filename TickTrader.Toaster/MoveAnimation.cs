using System;
using System.Windows;
using System.Windows.Media.Animation;

namespace TickTrader.Toaster
{
    internal class MoveAnimation
    {
        private Storyboard storyboard;
        private DoubleAnimation animationX;
        private DoubleAnimation animationY;
        private FrameworkElement element;

        public MoveAnimation(FrameworkElement element)
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));

            this.element = element;
            storyboard = new Storyboard();
            animationX = new DoubleAnimation();
            animationY = new DoubleAnimation();
            Storyboard.SetTargetProperty(animationY, new PropertyPath(Window.TopProperty));
            Storyboard.SetTargetProperty(animationX, new PropertyPath(Window.LeftProperty));
            storyboard.Children.Add(animationX);
            storyboard.Children.Add(animationY);
        }

        public TimeSpan Duration { get; set; }
        public Point From { get; set; }
        public Point To { get; set; }

        public void Start()
        {
            storyboard.Stop(element);

            animationX.From = this.From.X;
            animationX.To = this.To.X;
            animationX.Duration = this.Duration;
            animationY.From = this.From.Y;
            animationY.To = this.To.Y;
            animationY.Duration = this.Duration;

            storyboard.Begin(element, true);
        }

        public void Stop()
        {
            storyboard.Stop(element);
        }
    }
}
