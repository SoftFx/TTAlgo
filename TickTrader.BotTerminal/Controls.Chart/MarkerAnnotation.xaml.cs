using SciChart.Charting.Visuals.Annotations;
using System;
using System.Windows.Media;
using TickTrader.Algo.Api;
using TickTrader.Algo.CoreV1;
using TickTrader.Algo.Domain;

namespace TickTrader.BotTerminal
{
    /// <summary>
    /// Interaction logic for MarkerAnnotation.xaml
    /// </summary>
    public partial class MarkerAnnotation : CustomAnnotation
    {
        public MarkerAnnotation(Marker marker, Color stroke, double strokeThickness, Metadata.Types.MarkerSize size, DateTime x)
        {
            InitializeComponent();

            this.DataContext = marker;

            CoordinateMode = AnnotationCoordinateMode.Absolute;
            Y1 = marker.Y;
            X1 = x;
            IsHidden = double.IsNaN(marker.Y);
            ToolTip = marker.DisplayText;

            this.Icon.Data = (Geometry)FindResource(GetIconResxKey(marker.Icon));
            this.Icon.Stroke = new SolidColorBrush(stroke);
            this.Icon.StrokeThickness = strokeThickness;
            this.Icon.Fill = new SolidColorBrush(marker.Color.ToArgb(ApiColorConverter.GreenColor).ToWindowsColor());

            switch (size)
            {
                case Metadata.Types.MarkerSize.Large: Width = 24; Height = 24; break;
                case Metadata.Types.MarkerSize.Small: Width = 10; Height = 10; break;
                default: Width = 16; Height = 16; break;
            }
        }

        private static string GetIconResxKey(MarkerIcons icon)
        {
            switch (icon)
            {
                case MarkerIcons.Circle: return "MarkerCircleIcon";
                case MarkerIcons.UpArrow: return "MarkerUpArrowIcon";
                case MarkerIcons.DownArrow: return "MarkerDownArrowIcon";
                case MarkerIcons.UpTriangle: return "MarkerUpTriangleIcon";
                case MarkerIcons.DownTriangle: return "MarkerDownTriangleIcon";
                case MarkerIcons.Square: return "MarkerSquareIcon";
                case MarkerIcons.Diamond: return "MarkerDimondIcon";
                default: return "MarkerDimondIcon";
            }
        }
    }
}
