//using SciChart.Charting.Visuals.PointMarkers;
//using System.Collections.Generic;
//using SciChart.Drawing.Common;
//using System.Windows;
//using SciChart.Charting.Model.DataSeries;
//using System.ComponentModel;
//using System.Windows.Media;
//using NLog;
//using TickTrader.Algo.Domain;

//namespace TickTrader.BotTerminal
//{
//    public class AlgoPointMarker : BasePointMarker
//    {
//        private static Logger _logger = LogManager.GetCurrentClassLogger();


//        private IList<int> _dataPointIndexes = new List<int>();
//        private IPen2D _strokePen;

//        protected override GeometryHitTestResult HitTestCore(GeometryHitTestParameters hitTestParameters)
//        {
//            var result = base.HitTestCore(hitTestParameters);
//            return result;
//        }

//        protected override HitTestResult HitTestCore(PointHitTestParameters hitTestParameters)
//        {
//            var result = base.HitTestCore(hitTestParameters);
//            return result;
//        }

//        public override void BeginBatch(IRenderContext2D context, Color? strokeColor, Color? fillColor)
//        {
//            _dataPointIndexes.Clear();

//            base.BeginBatch(context, strokeColor, fillColor);
//        }

//        public override void MoveTo(IRenderContext2D context, double x, double y, int index)
//        {
//            if (IsInBounds(x, y))
//                _dataPointIndexes.Add(index);

//            base.MoveTo(context, x, y, index);
//        }

//        public override void Draw(IRenderContext2D context, IEnumerable<Point> centers)
//        {
//            InitResources(context);

//            var metadata = RenderableSeries.DataSeries.Metadata;

//            int i = 0;

//            foreach (Point center in centers)
//            {
//                var index = _dataPointIndexes[i++];
//                var pointMetadata = metadata[index] as AlgoMarkerMetadata;
//                if (pointMetadata == null)
//                {
//                    //_logger.Error($"{RenderableSeries.DataSeries.SeriesName}: No metadata at {index}");
//                    continue;
//                }
//                var fillColor = pointMetadata.MarkerEntity.ColorArgb.ToWindowsColor();
//                var newCenter = center;

//                //var newCenter = Shift(center, pointMetadata.MarkerEntity.Alignment);

//                using (var fillBrush = context.CreateBrush(fillColor))
//                {
//                    double iconWidth = Width;
//                    double iconHeight = Height / 2;

//                    switch (pointMetadata.MarkerEntity.Icon)
//                    {
//                        case MarkerInfo.Types.IconType.Diamond: DrawDiamond(context, newCenter, Width, iconHeight, _strokePen, fillBrush); break;
//                        case MarkerInfo.Types.IconType.Square: DrawSquare(context, newCenter, Width, iconHeight, _strokePen, fillBrush); break;
//                        case MarkerInfo.Types.IconType.Circle: DrawCircle(context, newCenter, Width, iconHeight, _strokePen, fillBrush); break;
//                        case MarkerInfo.Types.IconType.UpTriangle: DrawUpTriangle(context, newCenter, Width, iconHeight, _strokePen, fillBrush); break;
//                        case MarkerInfo.Types.IconType.DownTriangle: DrawDownTriangle(context, newCenter, Width, iconHeight, _strokePen, fillBrush); break;
//                        case MarkerInfo.Types.IconType.UpArrow: DrawUpArrow(context, newCenter, Width, iconHeight, _strokePen, fillBrush); break;
//                        case MarkerInfo.Types.IconType.DownArrow: DrawDownArrow(context, newCenter, Width, iconHeight, _strokePen, fillBrush); break;
//                    }
//                }
//            }
//        }

//        //private Point Shift(Point center, MarkerAlignments alignment)
//        //{
//        //    switch (alignment)
//        //    {
//        //        default: return center;
//        //        case MarkerAlignments.Bottom: return new Point(center.X, center.Y + Height);
//        //        case MarkerAlignments.Top: return new Point(center.X, center.Y - Height);
//        //    }
//        //}

//        private void DrawDiamond(IRenderContext2D context, Point center, double width, double height, IPen2D stroke, IBrush2D fill)
//        {
//            double top = center.Y - height;
//            double bottom = center.Y + height;
//            double left = center.X - width;
//            double right = center.X + width;

//            var diamondPoints = new[]
//                {
//                    new Point(center.X, top),
//                    new Point(right, center.Y),
//                    new Point(center.X, bottom),
//                    new Point(left, center.Y),
//                    new Point(center.X, top),
//                };

//            context.FillPolygon(fill, diamondPoints);
//            context.DrawLines(stroke, diamondPoints);
//        }

//        private void DrawUpTriangle(IRenderContext2D context, Point center, double width, double height, IPen2D stroke, IBrush2D fill)
//        {
//            double top = center.Y - height;
//            double bottom = center.Y + height;
//            double left = center.X - width;
//            double right = center.X + width;

//            var diamondPoints = new[]
//                {
//                    new Point(center.X, top),
//                    new Point(right, bottom),
//                    new Point(left, bottom),
//                    new Point(center.X, top),
//                };

//            context.FillPolygon(fill, diamondPoints);
//            context.DrawLines(stroke, diamondPoints);
//        }

//        private void DrawDownTriangle(IRenderContext2D context, Point center, double width, double height, IPen2D stroke, IBrush2D fill)
//        {
//            double top = center.Y - height;
//            double bottom = center.Y + height;
//            double left = center.X - width;
//            double right = center.X + width;

//            var diamondPoints = new[]
//                {
//                    new Point(center.X, bottom),
//                    new Point(right, top),
//                    new Point(left, top),
//                    new Point(center.X, bottom),
//                };

//            context.FillPolygon(fill, diamondPoints);
//            context.DrawLines(stroke, diamondPoints);
//        }

//        private void DrawUpArrow(IRenderContext2D context, Point center, double width, double height, IPen2D stroke, IBrush2D fill)
//        {
//            double top = center.Y - height;
//            double bottom = center.Y + height;
//            double left = center.X - width * 0.7;
//            double right = center.X + width * 0.7;
//            double leftBase = center.X - width * 0.4;
//            double rightBase = center.X + width * 0.4;

//            var diamondPoints = new[]
//                {
//                    new Point(center.X, top),
//                    new Point(right, center.Y),
//                    new Point(rightBase, center.Y),
//                    new Point(rightBase, bottom),
//                    new Point(leftBase, bottom),
//                    new Point(leftBase, center.Y),
//                    new Point(left, center.Y),
//                    new Point(center.X, top),
//                };

//            context.FillPolygon(fill, diamondPoints);
//            context.DrawLines(stroke, diamondPoints);
//        }

//        private void DrawDownArrow(IRenderContext2D context, Point center, double width, double height, IPen2D stroke, IBrush2D fill)
//        {
//            double top = center.Y - height;
//            double bottom = center.Y + height;
//            double left = center.X - width * 0.7;
//            double right = center.X + width * 0.7;
//            double leftBase = center.X - width * 0.4;
//            double rightBase = center.X + width * 0.4;

//            var diamondPoints = new[]
//                {
//                    new Point(center.X, bottom),
//                    new Point(right, center.Y),
//                    new Point(rightBase, center.Y),
//                    new Point(rightBase, top),
//                    new Point(leftBase, top),
//                    new Point(leftBase, center.Y),
//                    new Point(left, center.Y),
//                    new Point(center.X, bottom),
//                };

//            context.FillPolygon(fill, diamondPoints);
//            context.DrawLines(stroke, diamondPoints);
//        }

//        private void DrawCircle(IRenderContext2D context, Point center, double width, double height, IPen2D stroke, IBrush2D fill)
//        {
//            context.DrawEllipse(stroke, fill, center, width * 2, height * 2);
//        }

//        private void DrawSquare(IRenderContext2D context, Point center, double width, double height, IPen2D stroke, IBrush2D fill)
//        {
//            double top = center.Y - height;
//            double bottom = center.Y + height;
//            double left = center.X - width;
//            double right = center.X + width;

//            var p1 = new Point(left, top);
//            var p2 = new Point(right, bottom);

//            context.FillRectangle(fill, p1, p2);
//            context.DrawQuad(stroke, p1, p2);
//        }

//        private void InitResources(IRenderContext2D context)
//        {
//            _strokePen = _strokePen ?? context.CreatePen(Stroke, AntiAliasing, (float)StrokeThickness, Opacity);
//        }

//        public override void Dispose()
//        {
//            base.Dispose();
//            _strokePen?.Dispose();
//        }
//    }

//    public class AlgoMarkerMetadata : IPointMetadata
//    {
//        public AlgoMarkerMetadata(MarkerInfo marker)
//        {
//            MarkerEntity = marker;
//        }

//        public MarkerInfo MarkerEntity { get; private set; }
//        public bool IsSelected { get; set; }
//        public string Text { get { return MarkerEntity.DisplayText; } }

//        #pragma warning disable 67
//        public event PropertyChangedEventHandler PropertyChanged;
//    }
//}
