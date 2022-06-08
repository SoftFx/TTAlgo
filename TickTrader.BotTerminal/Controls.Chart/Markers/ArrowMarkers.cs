using LiveChartsCore.SkiaSharpView.Drawing.Geometries;
using SkiaSharp;

namespace TickTrader.BotTerminal.Controls.Chart.Markers
{
    internal sealed class ArrowUpMarker : SVGPathGeometry
    {
        private static readonly SKPath _svgPath = SKPath.ParseSvgPathData("M10 2.5L16.5 9H13v8H7V9H3.5L10 2.5z");

        public ArrowUpMarker() : base(_svgPath)
        {
        }
    }


    internal sealed class ArrowDownMarker : SVGPathGeometry
    {
        private static readonly SKPath _svgPath = SKPath.ParseSvgPathData("M10 17.5L3.5 11H7V3h6v8h3.5L10 17.5z");

        public ArrowDownMarker() : base(_svgPath)
        {
        }
    }
}