using SciChart.Charting.Model.DataSeries;
using SciChart.Charting.Visuals.PointMarkers;
using SciChart.Drawing.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using TickTrader.Algo.Api;

namespace TickTrader.BotTerminal
{
    internal class PositionMarker : BasePointMarker
    {
        //private static Logger _logger = LogManager.GetCurrentClassLogger();

        private IList<int> dataPointIndexes = new List<int>();
        private IPen2D strokePen;

        public Color SellMarkerColor { get; set; } = System.Windows.Media.Colors.Red;
        public Color BuyMarkerColor { get; set; } = System.Windows.Media.Colors.Blue;

        protected override GeometryHitTestResult HitTestCore(GeometryHitTestParameters hitTestParameters)
        {
            var result = base.HitTestCore(hitTestParameters);
            return result;
        }

        protected override HitTestResult HitTestCore(PointHitTestParameters hitTestParameters)
        {
            var result = base.HitTestCore(hitTestParameters);
            return result;
        }

        public override void BeginBatch(IRenderContext2D context, Color? strokeColor, Color? fillColor)
        {
            dataPointIndexes.Clear();

            base.BeginBatch(context, strokeColor, fillColor);
        }

        public override void MoveTo(IRenderContext2D context, double x, double y, int index)
        {
            if (IsInBounds(x, y))
                dataPointIndexes.Add(index);

            base.MoveTo(context, x, y, index);
        }

        public override void Draw(IRenderContext2D context, IEnumerable<Point> centers)
        {
            InitResources(context);

            double iconWidth = Width;
            double iconHeight = Height / 2;

            var metadata = RenderableSeries.DataSeries.Metadata;

            int i = 0;

            using (var sellBrush = context.CreateBrush(SellMarkerColor))
            {
                using (var buyBrush = context.CreateBrush(BuyMarkerColor))
                {
                    foreach (Point center in centers)
                    {
                        var index = dataPointIndexes[i++];
                        var pointMetadata = metadata[index] as PositionMarkerMetadatda;
                        if (pointMetadata == null)
                            continue;

                        if (pointMetadata.HasBuySide)
                        {
                            if (pointMetadata.HasSellSide)
                                DrawDiamond(context, center, iconWidth, iconHeight, strokePen, buyBrush, sellBrush);
                            else
                                DrawUpArrow(context, center, iconWidth, iconHeight, strokePen, buyBrush);
                        }
                        else
                            DrawDownArrow(context, center, iconWidth, iconHeight, strokePen, sellBrush);
                    }
                }
            }
        }

        private void DrawDiamond(IRenderContext2D context, Point center, double width, double height, IPen2D stroke, IBrush2D fillUp, IBrush2D fillDown)
        {
            double top = center.Y - height;
            double bottom = center.Y + height;
            double left = center.X - width;
            double right = center.X + width;

            var upTriangelPoints = new[]
                {
                    new Point(center.X, top),
                    new Point(right, center.Y),
                    new Point(left, center.Y),
                    new Point(center.X, top),
                };

            var downTrianglePoints = new[]
                {
                    new Point(left, center.Y),
                    new Point(right, center.Y),
                    new Point(center.X, bottom),
                    new Point(left, center.Y),
                };

            var diamondPoints = new[]
               {
                    new Point(center.X, top),
                    new Point(right, center.Y),
                    new Point(center.X, bottom),
                    new Point(left, center.Y),
                    new Point(center.X, top),
                };

            context.DrawLines(stroke, diamondPoints);
            context.FillPolygon(fillUp, upTriangelPoints);
            context.FillPolygon(fillDown, downTrianglePoints);
        }

        private void DrawUpArrow(IRenderContext2D context, Point center, double width, double height, IPen2D stroke, IBrush2D fill)
        {
            double top = center.Y - height;
            double bottom = center.Y + height;
            double left = center.X - width * 0.7;
            double right = center.X + width * 0.7;
            double leftBase = center.X - width * 0.4;
            double rightBase = center.X + width * 0.4;

            var diamondPoints = new[]
                {
                    new Point(center.X, top),
                    new Point(right, center.Y),
                    new Point(rightBase, center.Y),
                    new Point(rightBase, bottom),
                    new Point(leftBase, bottom),
                    new Point(leftBase, center.Y),
                    new Point(left, center.Y),
                    new Point(center.X, top),
                };

            context.FillPolygon(fill, diamondPoints);
            context.DrawLines(stroke, diamondPoints);
        }

        private void DrawDownArrow(IRenderContext2D context, Point center, double width, double height, IPen2D stroke, IBrush2D fill)
        {
            double top = center.Y - height;
            double bottom = center.Y + height;
            double left = center.X - width * 0.7;
            double right = center.X + width * 0.7;
            double leftBase = center.X - width * 0.4;
            double rightBase = center.X + width * 0.4;

            var diamondPoints = new[]
                {
                    new Point(center.X, bottom),
                    new Point(right, center.Y),
                    new Point(rightBase, center.Y),
                    new Point(rightBase, top),
                    new Point(leftBase, top),
                    new Point(leftBase, center.Y),
                    new Point(left, center.Y),
                    new Point(center.X, bottom),
                };

            context.FillPolygon(fill, diamondPoints);
            context.DrawLines(stroke, diamondPoints);
        }

        private void InitResources(IRenderContext2D context)
        {
            strokePen = strokePen ?? context.CreatePen(Stroke, AntiAliasing, (float)StrokeThickness, Opacity);
        }

        public override void Dispose()
        {
            base.Dispose();
            strokePen?.Dispose();
        }
    }

    internal class PositionMarkerMetadatda : IPointMetadata
    {
        private SortedDictionary<PosMarkerKey, string> _ordeEntires = new SortedDictionary<PosMarkerKey, string>();

        public PositionMarkerMetadatda(PosMarkerKey id, string description, bool isBuy)
        {
            AddRecord(id, description, isBuy);
        }

        public void AddRecord(PosMarkerKey id,  string description, bool isBuy)
        {
            if (isBuy)
                HasBuySide = true;
            else
                HasSellSide = true;

            _ordeEntires.Add(id, description);
        }

        public bool HasRecordFor(PosMarkerKey id)
        {
            return _ordeEntires.ContainsKey(id);
        }

        public bool HasSellSide { get; private set; }
        public bool HasBuySide { get; private set; }
        public bool IsSelected { get; set; }
        public string Text => BuildText();

        private string BuildText()
        {
            return string.Join(Environment.NewLine, _ordeEntires.Values);
        }

        #pragma warning disable 67
        public event PropertyChangedEventHandler PropertyChanged;
    }

    internal class PosMarkerKey : IComparable<PosMarkerKey>
    {
        public PosMarkerKey(string orderId, string actionId = null)
        {
            OrderId = long.Parse(orderId);
            ActionId = actionId;
        }

        public long OrderId { get; }
        public string ActionId { get; }

        int IComparable<PosMarkerKey>.CompareTo(PosMarkerKey other)
        {
            var idCompare = OrderId.CompareTo(other.OrderId);
            if (idCompare != 0)
                return idCompare;
            return string.Compare(ActionId, other.ActionId);
        }
    }
}
