using TickTrader.Algo.Api;
using TickTrader.Algo.Api.Ext;

namespace TickTrader.Algo.Ext
{
    // Implemented internally
    //[Reduction("Bid")]
    public class BidBarReduction : FullBarToBarReduction
    {
        public void Reduce(Bar bidBar, Bar askBar, IBarWriter result)
        {
            // Just for example purposes. Internally works like: 
            // return bidBar;

            result.Open = bidBar.Open;
            result.Close = bidBar.Close;
            result.High = bidBar.High;
            result.Low = bidBar.Low;
            result.Volume = bidBar.Volume;
        }
    }


    // Implemented internally
    //[Reduction("Ask")]
    public class AskBarReduction : FullBarToBarReduction
    {
        public void Reduce(Bar bidBar, Bar askBar, IBarWriter result)
        {
            // Just for example purposes. Internally works like: 
            // return askBar;

            result.Open = askBar.Open;
            result.Close = askBar.Close;
            result.High = askBar.High;
            result.Low = askBar.Low;
            result.Volume = askBar.Volume;
        }
    }


    [Reduction("Forex")]
    public class ForexBarReduction : FullBarToBarReduction
    {
        public void Reduce(Bar bidBar, Bar askBar, IBarWriter result)
        {
            result.Open = (bidBar.Open + askBar.Open) / 2;
            result.Close = (bidBar.Close + askBar.Close) / 2;
            result.High = askBar.High;
            result.Low = bidBar.Low;
            result.Volume = bidBar.Volume + askBar.Volume;
        }
    }
}
