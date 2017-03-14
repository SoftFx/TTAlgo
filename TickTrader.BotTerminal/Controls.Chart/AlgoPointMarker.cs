﻿using SciChart.Charting.Visuals.PointMarkers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SciChart.Drawing.Common;
using System.Windows;
using SciChart.Charting.Model.DataSeries;
using System.ComponentModel;
using TickTrader.Algo.Api;
using System.Windows.Media;

namespace TickTrader.BotTerminal
{
    public class AlgoPointMarker : BasePointMarker
    {
        private IList<int> dataPointIndexes = new List<int>();
        private IPen2D strokePen;

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

            var metadata = RenderableSeries.DataSeries.Metadata;

            int i = 0;

            foreach (Point center in centers)
            {
                var pointMetadata = metadata[dataPointIndexes[i++]] as AlgoMarkerMetadata;
                var fillColor = Algo.GuiModel.Convert.ToWindowsColor(pointMetadata.MarkerEntity.Color, Stroke);
                var newCenter = center;

                //var newCenter = Shift(center, pointMetadata.MarkerEntity.Alignment);

                using (var fillBrush = context.CreateBrush(fillColor))
                {
                    double iconWidth = Width;
                    double iconHeight = Height / 2;

                    switch (pointMetadata.MarkerEntity.Icon)
                    {
                        case MarkerIcons.Diamond: DrawDiamond(context, newCenter, Width, iconHeight, strokePen, fillBrush); break;
                        case MarkerIcons.Square: DrawSquare(context, newCenter, Width, iconHeight, strokePen, fillBrush); break;
                        case MarkerIcons.Circle: DrawCircle(context, newCenter, Width, iconHeight, strokePen, fillBrush); break;
                        case MarkerIcons.UpTriangle: DrawUpTriangle(context, newCenter, Width, iconHeight, strokePen, fillBrush); break;
                        case MarkerIcons.DownTriangle: DrawDownTriangle(context, newCenter, Width, iconHeight, strokePen, fillBrush); break;
                        case MarkerIcons.UpArrow: DrawUpArrow(context, newCenter, Width, iconHeight, strokePen, fillBrush); break;
                        case MarkerIcons.DownArrow: DrawDownArrow(context, newCenter, Width, iconHeight, strokePen, fillBrush); break;
                    }
                }
            }
        }

        //private Point Shift(Point center, MarkerAlignments alignment)
        //{
        //    switch (alignment)
        //    {
        //        default: return center;
        //        case MarkerAlignments.Bottom: return new Point(center.X, center.Y + Height);
        //        case MarkerAlignments.Top: return new Point(center.X, center.Y - Height);
        //    }
        //}

        private void DrawDiamond(IRenderContext2D context, Point center, double width, double height, IPen2D stroke, IBrush2D fill)
        {
            double top = center.Y - height;
            double bottom = center.Y + height;
            double left = center.X - width;
            double right = center.X + width;

            var diamondPoints = new[]
                {
                    new Point(center.X, top),
                    new Point(right, center.Y),
                    new Point(center.X, bottom),
                    new Point(left, center.Y),
                    new Point(center.X, top),
                };

            context.FillPolygon(fill, diamondPoints);
            context.DrawLines(stroke, diamondPoints);
        }

        private void DrawUpTriangle(IRenderContext2D context, Point center, double width, double height, IPen2D stroke, IBrush2D fill)
        {
            double top = center.Y - height;
            double bottom = center.Y + height;
            double left = center.X - width;
            double right = center.X + width;

            var diamondPoints = new[]
                {
                    new Point(center.X, top),
                    new Point(right, bottom),
                    new Point(left, bottom),
                    new Point(center.X, top),
                };

            context.FillPolygon(fill, diamondPoints);
            context.DrawLines(stroke, diamondPoints);
        }

        private void DrawDownTriangle(IRenderContext2D context, Point center, double width, double height, IPen2D stroke, IBrush2D fill)
        {
            double top = center.Y - height;
            double bottom = center.Y + height;
            double left = center.X - width;
            double right = center.X + width;

            var diamondPoints = new[]
                {
                    new Point(center.X, bottom),
                    new Point(right, top),
                    new Point(left, top),
                    new Point(center.X, bottom),
                };

            context.FillPolygon(fill, diamondPoints);
            context.DrawLines(stroke, diamondPoints);
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

        private void DrawCircle(IRenderContext2D context, Point center, double width, double height, IPen2D stroke, IBrush2D fill)
        {
            context.DrawEllipse(stroke, fill, center, width * 2, height * 2);
        }

        private void DrawSquare(IRenderContext2D context, Point center, double width, double height, IPen2D stroke, IBrush2D fill)
        {
            double top = center.Y - height;
            double bottom = center.Y + height;
            double left = center.X - width;
            double right = center.X + width;

            var p1 = new Point(left, top);
            var p2 = new Point(right, bottom);

            context.FillRectangle(fill, p1, p2);
            context.DrawQuad(stroke, p1, p2);
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

    public class AlgoMarkerMetadata : IPointMetadata
    {
        public AlgoMarkerMetadata(Marker marker)
        {
            this.MarkerEntity = marker;
        }

        public Marker MarkerEntity { get; private set; }
        public bool IsSelected { get; set; }
        public string Text { get { return MarkerEntity.DisplayText; } }

        #pragma warning disable 67
        public event PropertyChangedEventHandler PropertyChanged;
    }
}