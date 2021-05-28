using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace TickTrader.BotTerminal
{
    public class CoordinatePicker : Control
    {
        private const string PanelCoordinateName = "PART_CanvasCoordinates";

        private TranslateTransform markerPositionTransform = new TranslateTransform();
        private FrameworkElement canvasCoordinates;
        private bool isUpdatingCoordinates;

        public static readonly DependencyProperty CoordinateXProperty =
            DependencyProperty.Register("CoordinateX", typeof(double), typeof(CoordinatePicker), new PropertyMetadata(0.0d, (d, e) => ((CoordinatePicker)d).CalculateMarkerPosition()));

        public static readonly DependencyProperty CoordinateYProperty =
            DependencyProperty.Register("CoordinateY", typeof(double), typeof(CoordinatePicker), new PropertyMetadata(0.0d, (d, e) => ((CoordinatePicker)d).CalculateMarkerPosition()));

        public static readonly DependencyProperty MaxXProperty =
            DependencyProperty.Register("MaxX", typeof(double), typeof(CoordinatePicker), new PropertyMetadata(100.0d));

        public static readonly DependencyProperty MaxYProperty =
            DependencyProperty.Register("MaxY", typeof(double), typeof(CoordinatePicker), new PropertyMetadata(100.0d));

        public static readonly DependencyProperty MarkerXProperty =
            DependencyProperty.Register("MarkerX", typeof(double), typeof(CoordinatePicker));

        public static readonly DependencyProperty MarkerYProperty =
            DependencyProperty.Register("MarkerY", typeof(double), typeof(CoordinatePicker));

        public static readonly DependencyProperty IsDirectionReversedProperty =
            DependencyProperty.Register(nameof(IsDirectionReversed), typeof(bool), typeof(CoordinatePicker), new PropertyMetadata(false));


        public bool IsDirectionReversed
        {
            get { return (bool)GetValue(IsDirectionReversedProperty); }
            set { SetValue(IsDirectionReversedProperty, value); }
        }

        public double CoordinateX
        {
            get { return (double)GetValue(CoordinateXProperty); }
            set
            {
                SetValue(CoordinateXProperty, value);
            }
        }

        public double CoordinateY
        {
            get { return (double)GetValue(CoordinateYProperty); }
            set { SetValue(CoordinateYProperty, value); }
        }

        public double MaxY
        {
            get { return (double)GetValue(MaxYProperty); }
            set { SetValue(MaxYProperty, value); }
        }

        public double MaxX
        {
            get { return (double)GetValue(MaxXProperty); }
            set { SetValue(MaxXProperty, value); }
        }

        public double MarkerX
        {
            get { return (double)GetValue(MarkerXProperty); }
            set { SetValue(MarkerXProperty, value); }
        }

        public double MarkerY
        {
            get { return (double)GetValue(MarkerYProperty); }
            set { SetValue(MarkerYProperty, value); }
        }

        static CoordinatePicker()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CoordinatePicker), new FrameworkPropertyMetadata(typeof(CoordinatePicker)));
        }

        public CoordinatePicker()
        {

        }
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            canvasCoordinates = GetTemplateChild(PanelCoordinateName) as FrameworkElement;
            canvasCoordinates.MouseLeftButtonDown += CanvasCoordinates_MouseLeftButtonDown;
            canvasCoordinates.PreviewMouseMove += CanvasCoordinates_MouseMove;
            canvasCoordinates.SizeChanged += CanvasCoordinates_SizeChanged;
        }

        private void CanvasCoordinates_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var currentMousePosition = e.GetPosition(canvasCoordinates);

                updateMarkerPosition(currentMousePosition);
            }
        }

        private void CanvasCoordinates_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var currentMousePosition = e.GetPosition(canvasCoordinates);
            updateMarkerPosition(currentMousePosition);
        }

        protected override void OnTemplateChanged(ControlTemplate oldTemplate, ControlTemplate newTemplate)
        {
            if (oldTemplate != null)
            {
                canvasCoordinates.MouseLeftButtonDown -= CanvasCoordinates_MouseLeftButtonDown;
                canvasCoordinates.PreviewMouseMove -= CanvasCoordinates_MouseMove;
                canvasCoordinates.SizeChanged -= CanvasCoordinates_SizeChanged;
                canvasCoordinates = null;
            }
        }

        private void CanvasCoordinates_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            CalculateMarkerPosition();
        }

        private void CalculateMarkerPosition()
        {
            if (canvasCoordinates != null)
                if (!isUpdatingCoordinates)
                {
                    if (canvasCoordinates.ActualWidth > 0)
                        MarkerX = CoordinateX / (MaxX / canvasCoordinates.ActualWidth);
                    if (canvasCoordinates.ActualHeight > 0)
                        MarkerY = CoordinateY / (MaxY / canvasCoordinates.ActualHeight);
                }
        }

        private void CalculateCoordinates()
        {
            isUpdatingCoordinates = true;
            if (canvasCoordinates.ActualWidth > 0)
                CoordinateX = MarkerX * (MaxX / canvasCoordinates.ActualWidth);
            if (canvasCoordinates.ActualHeight > 0)
                CoordinateY = MarkerY * (MaxY / canvasCoordinates.ActualHeight);
            isUpdatingCoordinates = false;
        }

        private void updateMarkerPosition(Point point)
        {
            if (point.X < 0)
                MarkerX = 0;
            else if (point.X > canvasCoordinates.ActualWidth)
                MarkerX = canvasCoordinates.ActualWidth;
            else
                MarkerX = point.X;

            if (point.Y < 0)
                MarkerY = 0;
            else if (point.Y > canvasCoordinates.ActualHeight)
                MarkerY = canvasCoordinates.ActualHeight;
            else
                MarkerY = point.Y;

            CalculateCoordinates();
        }
    }
}
