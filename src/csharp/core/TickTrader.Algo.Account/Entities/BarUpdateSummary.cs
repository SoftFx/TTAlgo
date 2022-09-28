using System;
using System.Text;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Account
{
    public class BarUpdateSummary
    {
        public string Symbol { get; set; }

        public double? AskClose { get; set; }

        public double? BidClose { get; set; }

        public BarUpdateDetails[] Details { get; set; }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"{Symbol}, ask = {AskClose}, bid = {BidClose}");
            if (Details?.Length > 0)
            {
                foreach(var d in Details)
                {
                    sb.AppendLine($"{d.Timeframe}, {d.MarketSide}: from={d.From}, open={d.Open}, high={d.High}, low={d.Low}");
                }
            }
            return sb.ToString();
        }
    }


    public class BarUpdateDetails
    {
        public Feed.Types.Timeframe Timeframe { get; set; }

        public Feed.Types.MarketSide MarketSide { get; set; }

        public DateTime? From { get; set; }

        public double? Open { get; set; }

        public double? High { get; set; }

        public double? Low { get; set; }
    }
}
