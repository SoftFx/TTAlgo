using TickTrader.Algo.Api;

namespace TickTrader.Algo.Indicators.ATCFMethod.FTLMSTLM
{
    [Indicator(Category = "AT&CF Method", DisplayName = "AT&CF Method/FTLM-STLM")]
    public class FtlmStlm : Indicator
    {
        [Input]
        public DataSeries Price { get; set; }

        [Output(DisplayName = "FTLM", DefaultColor = Colors.DarkKhaki)]
        public DataSeries Ftlm { get; set; }

        [Output(DisplayName = "STLM", DefaultColor = Colors.DarkSalmon)]
        public DataSeries Stlm { get; set; }

        public int LastPositionChanged { get { return 0; } }

        public FtlmStlm() { }

        public FtlmStlm(DataSeries price)
        {
            Price = price;

            InitializeIndicator();
        }

        private void InitializeIndicator() { }

        protected override void Init()
        {
            InitializeIndicator();
        }

        protected override void Calculate()
        {
            
        }
    }
}
