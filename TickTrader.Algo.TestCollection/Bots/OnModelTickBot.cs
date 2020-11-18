using System.Text;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.TestCollection.Bots
{
    [TradeBot(DisplayName = "[T] On Model Tick Bot", Version = "1.0", Category = "Test Bot Routine",
        Description = "Prints when OnModelTick is called")]
    public class OnModelTickBot : TradeBot
    {
        private BarSeries _barsBid, _barsAsk;


        [Parameter(DefaultValue = "GBPUSD")]
        public string AuxSymbol { get; set; }


        protected override void Init()
        {
            _barsBid = Feed.GetBarSeries(AuxSymbol, BarPriceType.Bid);
        }

        protected override void OnStart()
        {
            _barsAsk = Feed.GetBarSeries(AuxSymbol, BarPriceType.Ask);
        }


        protected override void OnModelTick()
        {
            var sb = new StringBuilder();
            sb.Append($"OnModelTick: {Now:yyyy-MM-dd HH:mm:ss.fff}");
            Print(sb.ToString());
            sb.AppendLine();
            
            var bar = Bars[0];
            sb.AppendLine($"Last {Symbol.Name} bar:");
            sb.AppendLine($"IsNull - {bar.IsNull}");
            sb.AppendLine($"Open time - {bar.OpenTime:yyyy-MM-dd HH:mm:ss.fff}");
            sb.AppendLine($"Close time - {bar.CloseTime:yyyy-MM-dd HH:mm:ss.fff}");
            sb.AppendLine($"Open - {bar.Open}");
            sb.AppendLine($"High - {bar.High}");
            sb.AppendLine($"Low - {bar.Low}");
            sb.AppendLine($"Close - {bar.Close}");
            
            bar = _barsBid[0];
            sb.AppendLine($"Last {AuxSymbol} bid bar:");
            sb.AppendLine($"IsNull - {bar.IsNull}");
            sb.AppendLine($"Open time - {bar.OpenTime:yyyy-MM-dd HH:mm:ss.fff}");
            sb.AppendLine($"Close time - {bar.CloseTime:yyyy-MM-dd HH:mm:ss.fff}");
            sb.AppendLine($"Open - {bar.Open}");
            sb.AppendLine($"High - {bar.High}");
            sb.AppendLine($"Low - {bar.Low}");
            sb.AppendLine($"Close - {bar.Close}");

            bar = _barsAsk[0];
            sb.AppendLine($"Last {AuxSymbol} ask bar:");
            sb.AppendLine($"IsNull - {bar.IsNull}");
            sb.AppendLine($"Open time - {bar.OpenTime:yyyy-MM-dd HH:mm:ss.fff}");
            sb.AppendLine($"Close time - {bar.CloseTime:yyyy-MM-dd HH:mm:ss.fff}");
            sb.AppendLine($"Open - {bar.Open}");
            sb.AppendLine($"High - {bar.High}");
            sb.AppendLine($"Low - {bar.Low}");
            sb.AppendLine($"Close - {bar.Close}");

            Status.WriteLine(sb.ToString());
        }
    }
}
