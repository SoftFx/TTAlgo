using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;

namespace TickTrader.BotTerminal.Controls.Chart
{
    internal sealed class YLabelAxis : Axis
    {
        private readonly Axis _main;


        internal YLabelAxis(Axis mainAxis)
        {
            _main = mainAxis;

            IsVisible = false;
            Position = AxisPosition.End;
        }


        internal void SyncWithMain()
        {
            var bounds = _main.VisibleDataBounds;

            MaxLimit = bounds.Max;
            MinLimit = bounds.Min;
        }
    }
}
