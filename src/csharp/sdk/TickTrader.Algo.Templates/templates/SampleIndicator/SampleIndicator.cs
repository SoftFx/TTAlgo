using TickTrader.Algo.Api;

namespace SampleIndicator
{
    [Indicator(Category = "My indicators", DisplayName = "SampleIndicator", Version = "1.0",
        Description = "My awesome SampleIndicator")]
    public class SampleIndicator : Indicator
    {
        private bool _initFailed;


        [Parameter(DisplayName = "Period", DefaultValue = 14)]
        public int Period { get; set; }

        [Parameter(DisplayName = "Smooth Factor", DefaultValue = 0.0667)]
        public double SmoothFactor { get; set; }

        public enum CalcMode { Simple, Exponential }

        [Parameter(DisplayName = "Calculation Mode", DefaultValue = CalcMode.Simple)]
        public CalcMode Mode { get; set; }

        [Input(DisplayName = "Price Input")]
        public DataSeries Input { get; set; }

        [Output(DisplayName = "Average", Target = OutputTargets.Overlay, DefaultColor = Colors.Red)]
        public DataSeries Output { get; set; }


        protected override void Init()
        {
            if (Mode == CalcMode.Exponential && (SmoothFactor <= 0.0 || SmoothFactor >= 1.0))
            {
                PrintError("Invalid smooth factor");
                _initFailed = true;
            }
        }

        protected override void Calculate(bool isNewBar)
        {
            if (_initFailed)
                return;

            if (Input.Count < Period)
            {
                Output[0] = double.NaN;
            }
            else
            {
                var res = 0.0;
                switch (Mode)
                {
                    case CalcMode.Simple:
                        for (var i = 0; i < Period; i++)
                        {
                            res += Input[i];
                        }
                        res /= Period;
                        break;
                    case CalcMode.Exponential:
                        res = Input[Period];
                        for (var i = Period - 1; i >= 0; i--)
                        {
                            res = (1 - SmoothFactor) * res + SmoothFactor * Input[i];
                        }
                        break;
                }

                Output[0] = res;
            }
        }
    }
}
