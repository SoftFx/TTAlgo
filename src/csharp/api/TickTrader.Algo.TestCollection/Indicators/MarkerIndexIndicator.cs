using TickTrader.Algo.Api;

namespace TickTrader.Algo.TestCollection.Indicators
{
    [Indicator(Category = "Test Indicator Routine", DisplayName = "[T] Marker Index Test", Version = "1.0")]
    public class MarkerIndexIndicator : Indicator
    {
        private bool _batchBuildFinished;


        [Output(DisplayName = "Markers", Target = OutputTargets.Overlay, DefaultColor = Colors.Green)]
        public MarkerSeries Markers { get; set; }


        protected override void Calculate(bool isNewBar)
        {
            if (isNewBar && !_batchBuildFinished)
                _batchBuildFinished = true;

            if (!isNewBar)
                return;

            if (!_batchBuildFinished)
            {
                UpdateMarker(0);
            }
            else
            {
                for (var i = 0; i < Bars.Count; i++)
                {
                    UpdateMarker(i);
                }
            }
        }

        private void UpdateMarker(int i)
        {
            var marker = Markers[i];
            marker.Y = Bars[i].High + 5 * Symbol.Point;
        }
    }
}
