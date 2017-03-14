﻿using TickTrader.Algo.Api;

namespace TickTrader.Algo.Indicators.BillWilliams.Fractals
{
    [Indicator(IsOverlay = true, Category = "Bill Williams", DisplayName = "Bill Williams/Fractals")]
    public class Fractals : Indicator
    {
        [Input]
        public new BarSeries Bars { get; set; }

        [Output(DisplayName = "Fractals Up", DefaultColor = Colors.Gray)]
        public DataSeries<Marker> FractalsUp { get; set; }

        [Output(DisplayName = "Fractals Down", DefaultColor = Colors.Gray)]
        public DataSeries<Marker> FractalsDown { get; set; }

        public Fractals()
        {
        }

        public Fractals(BarSeries bars)
        {
            Bars = bars;

            InitializeIndicator();
        }

        private void InitializeIndicator()
        {

        }

        protected override void Init()
        {
            InitializeIndicator();
        }

        private bool IsFractalsUp(int i)
        {
            if (Bars.Count >= i + 3)
            {
                if (Bars[i].High > Bars[i + 1].High && Bars[i].High > Bars[i + 2].High &&
                    Bars[i].High > Bars[i - 1].High && Bars[i].High > Bars[i - 2].High)
                {
                    return true;
                }
            }
            if (Bars.Count >= i + 4)
            {
                if (Bars[i].High >= Bars[i + 1].High && Bars[i].High > Bars[i + 2].High &&
                    Bars[i].High > Bars[i + 3].High && Bars[i].High > Bars[i - 1].High &&
                    Bars[i].High > Bars[i - 2].High)
                {
                    return true;
                }
            }
            if (Bars.Count >= i + 5)
            {
                if (Bars[i].High >= Bars[i + 1].High && Bars[i].High >= Bars[i + 2].High &&
                    Bars[i].High > Bars[i + 3].High && Bars[i].High > Bars[i + 4].High &&
                    Bars[i].High > Bars[i - 1].High && Bars[i].High > Bars[i - 2].High)
                {
                    return true;
                }
            }
            if (Bars.Count >= i + 6)
            {
                if (Bars[i].High >= Bars[i + 1].High && Bars[i].High >= Bars[i + 2].High &&
                    Bars[i].High >= Bars[i + 3].High && Bars[i].High > Bars[i + 4].High &&
                    Bars[i].High > Bars[i + 5].High && Bars[i].High > Bars[i - 1].High &&
                    Bars[i].High > Bars[i - 2].High)
                {
                    return true;
                }
            }
            if (Bars.Count >= i + 7)
            {
                if (Bars[i].High >= Bars[i + 1].High && Bars[i].High >= Bars[i + 2].High &&
                    Bars[i].High >= Bars[i + 3].High && Bars[i].High >= Bars[i + 4].High &&
                    Bars[i].High > Bars[i + 5].High && Bars[i].High > Bars[i + 6].High &&
                    Bars[i].High > Bars[i - 1].High && Bars[i].High > Bars[i - 2].High)
                {
                    return true;
                }
            }
            return false;
        }

        private bool IsFractalsDown(int i)
        {
            if (Bars.Count >= i + 3)
            {
                if (Bars[i].Low < Bars[i + 1].Low && Bars[i].Low < Bars[i + 2].Low && Bars[i].Low < Bars[i - 1].Low &&
                    Bars[i].Low < Bars[i - 2].Low)
                {
                    return true;
                }
            }
            if (Bars.Count >= i + 4)
            {
                if (Bars[i].Low <= Bars[i + 1].Low && Bars[i].Low < Bars[i + 2].Low && Bars[i].Low < Bars[i + 3].Low &&
                    Bars[i].Low < Bars[i - 1].Low && Bars[i].Low < Bars[i - 2].Low)
                {
                    return true;
                }
            }
            if (Bars.Count >= i + 5)
            {
                if (Bars[i].Low <= Bars[i + 1].Low && Bars[i].Low <= Bars[i + 2].Low && Bars[i].Low < Bars[i + 3].Low &&
                    Bars[i].Low < Bars[i + 4].Low && Bars[i].Low < Bars[i - 1].Low && Bars[i].Low < Bars[i - 2].Low)
                {
                    return true;
                }
            }
            if (Bars.Count >= i + 6)
            {
                if (Bars[i].Low <= Bars[i + 1].Low && Bars[i].Low <= Bars[i + 2].Low && Bars[i].Low <= Bars[i + 3].Low &&
                    Bars[i].Low < Bars[i + 4].Low && Bars[i].Low < Bars[i + 5].Low && Bars[i].Low < Bars[i - 1].Low &&
                    Bars[i].Low < Bars[i - 2].Low)
                {
                    return true;
                }
            }
            if (Bars.Count >= i + 7)
            {
                if (Bars[i].Low <= Bars[i + 1].Low && Bars[i].Low <= Bars[i + 2].Low && Bars[i].Low <= Bars[i + 3].Low &&
                    Bars[i].Low <= Bars[i + 4].Low && Bars[i].Low < Bars[i + 5].Low && Bars[i].Low < Bars[i + 6].Low &&
                    Bars[i].Low < Bars[i - 1].Low && Bars[i].Low < Bars[i - 2].Low)
                {
                    return true;
                }
            }
            return false;
        }

        protected override void Calculate()
        {
            var i = 2;

            if (IsFractalsUp(i))
                SetMarker(i, Bars[i].High, true);
            else if(IsUpdate)
                ClearMarker(i, true);

            if (IsFractalsDown(i))
                SetMarker(i, Bars[i].Low, false);
            else if (IsUpdate)
                ClearMarker(i, false);
        }

        private void SetMarker(int pos, double level, bool up)
        {
            var marker = up ? FractalsUp[pos] : FractalsDown[pos];
            marker.Y = level;
            marker.Icon = up ? MarkerIcons.UpTriangle : MarkerIcons.DownTriangle;
            marker.DisplayText = (up ? "Fractal Up " : "Fractal Down ") + level;
        }

        private void ClearMarker(int pos, bool up)
        {
            var marker = up ? FractalsUp[pos] : FractalsDown[pos];
            marker.Clear();
        }
    }
}