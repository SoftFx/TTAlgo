using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media.Animation;

namespace TickTrader.Toaster
{
    class ToastHelper
    {
        internal static IToastSorter GetSorter(ToastPosition position)
        {
            switch (position)
            {
                case ToastPosition.BottomLeft:
                    return new BottomLeftSorter();
                case ToastPosition.TopLeft:
                    return new TopLeftSorter();
                case ToastPosition.BottomRight:
                    return new BottomRightSorter();
                case ToastPosition.TopRight:
                    return new TopRightSorter();
                default:
                    throw new ArgumentOutOfRangeException(nameof(position));
            }
        }

        internal static Storyboard GetFadeOutAnimation(TimeSpan duration, ref Grid toasterPlace)
        {
            var animationDelay = duration.TotalSeconds * 0.3;
            var animationDuration = duration.TotalSeconds - animationDelay;

            DoubleAnimation fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromSeconds(animationDuration))
            {
                BeginTime = TimeSpan.FromSeconds(animationDelay)
            };

            Storyboard.SetTargetProperty(fadeOut, new PropertyPath(Grid.OpacityProperty));

            var story = new Storyboard();
            story.Children.Add(fadeOut);
            return story;
        }
    }

    internal interface IToastSorter
    {
        void SetPosition(IToast toast, int positionNumber, bool animate);
        Point StartPosition { get; }
    }

    internal class BottomLeftSorter : IToastSorter
    {
        public Point StartPosition
        {
            get
            {
                var workingArea = Screen.PrimaryScreen.WorkingArea;
                return new Point(workingArea.Left, workingArea.Bottom);
            }
        }

        public void SetPosition(IToast toast, int positionNumber, bool animate)
        {
            
            var toastPosition = new Point(StartPosition.X, StartPosition.Y - toast.ActualHeight * (positionNumber));
            toast.Move(toastPosition, animate);
        }
    }

    internal class TopLeftSorter : IToastSorter
    {
        public Point StartPosition
        {
            get
            {
                var workingArea = Screen.PrimaryScreen.WorkingArea;
                return new Point(workingArea.Left, workingArea.Top);
            }
        }

        public void SetPosition(IToast toast, int positionNumber, bool animate)
        {
            var toastPosition = new Point(StartPosition.X, StartPosition.Y + toast.ActualHeight * (positionNumber));
            toast.Move(toastPosition, animate);
        }
    }

    internal class BottomRightSorter : IToastSorter
    {
        public Point StartPosition
        {
            get
            {
                var workingArea = Screen.PrimaryScreen.WorkingArea;
                return new Point(workingArea.Right, workingArea.Bottom);
            }
        }

        public void SetPosition(IToast toast, int positionNumber, bool animate)
        {
            
            var toastPosition = new Point(StartPosition.X - toast.ActualWidth, StartPosition.Y - toast.ActualHeight * (positionNumber));
            toast.Move(toastPosition, animate);
        }
    }

    internal class TopRightSorter : IToastSorter
    {
        public Point StartPosition
        {
            get
            {
                var workingArea = Screen.PrimaryScreen.WorkingArea;
                return new Point(workingArea.Right, workingArea.Top);
            }
        }

        public void SetPosition(IToast toast, int positionNumber, bool animate)
        {
            
            var toastPosition = new Point(StartPosition.X - toast.ActualWidth, StartPosition.Y + toast.ActualHeight * (positionNumber));
            toast.Move(toastPosition, animate);
        }
    }
}
