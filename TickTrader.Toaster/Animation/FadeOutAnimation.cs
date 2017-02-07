using System;
using System.Windows;
using System.Windows.Media.Animation;

namespace TickTrader.Toaster.Animation
{
    public class FadeOutAnimation : ICloseAnimation
    {
        private FrameworkElement element;
        private DoubleAnimation opacityAnimation;
        private Storyboard storyboard;

        public TimeSpan Duration { get; set; }

        public event Action Completed;

        public FadeOutAnimation()
        {
            storyboard = new Storyboard();
            storyboard.Completed += StoryboardCompleted;
            opacityAnimation = new DoubleAnimation();
            Storyboard.SetTargetProperty(opacityAnimation, new PropertyPath(FrameworkElement.OpacityProperty));
            storyboard.Children.Add(opacityAnimation);
        }

        public void Start(FrameworkElement element)
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));

            this.element = element;

            opacityAnimation.From = 1;
            opacityAnimation.To = 0;
            var animationDelay = Duration.TotalMilliseconds * 0.5;
            var animationDuration = Duration.TotalMilliseconds - animationDelay;
            opacityAnimation.Duration = TimeSpan.FromMilliseconds(animationDuration);
            opacityAnimation.BeginTime = TimeSpan.FromMilliseconds(animationDelay);

            storyboard.Begin(this.element, true);
        }

        private void StoryboardCompleted(object sender, EventArgs e)
        {
            Completed();
        }

        public void Stop()
        {
            if (element != null)
                storyboard.Stop(this.element);
        }

        public void Dispose()
        {
            Completed = null;
            storyboard.Completed -= StoryboardCompleted;
            Stop();
        }
    }
}
