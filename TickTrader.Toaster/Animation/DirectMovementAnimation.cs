using System;
using System.Windows;
using System.Windows.Media.Animation;


namespace TickTrader.Toaster.Animation
{
    class DirectMovementAnimation: IMoveAnimation
    {
        private Storyboard storyboard;
        private DoubleAnimation animationX;
        private DoubleAnimation animationY;
        private FrameworkElement element;

        public event Action Completed = delegate { };

        public DirectMovementAnimation()
        {
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

        public void Start(FrameworkElement element)
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));

            this.element = element;

            animationX.From = this.From.X;
            animationX.To = this.To.X;
            animationX.Duration = this.Duration;
            animationY.From = this.From.Y;
            animationY.To = this.To.Y;
            animationY.Duration = this.Duration;

            storyboard.Begin(this.element, true);
        }

        public void Stop()
        {
            if (element != null)
                storyboard.Stop(element);
        }

        public void Dispose()
        {
            
        }
    }
}
