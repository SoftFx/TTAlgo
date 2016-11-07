using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMMBot
{
    public class LMMBotTOMLConfiguration
    {
        public Dictionary<string, double> LPSymbols { get; set; }
        public double MarkupInPercent { get; set; }
        public bool DebugStatus { get; set; }
        public string BotTag { get; set; }
        public bool AutoAddVolume2PartialFill { get; set; }
        public bool AutoUpdate2TradeEvent { get; set; }
        public int MaxPriceRandomDiff { get; set; }

        public LMMBotTOMLConfiguration()
        {
            Dictionary<string, double>  t = new Dictionary<string, double>();
            t.Add("EMC/USD", 2);
            LPSymbols = t;
            MarkupInPercent = 1;
            DebugStatus = false;
            BotTag = "12345";

        }
        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            foreach (KeyValuePair<string, double> pair in LPSymbols)
                builder.AppendLine(SymbolNameConvertor(pair.Key) + " = " + pair.Value);
            builder.AppendLine("MarkupInPercent = " + MarkupInPercent);
            builder.AppendLine("DebugStatus = " + DebugStatus);
            builder.AppendLine("BotTag = " + BotTag);
            builder.AppendLine("AutoAddVolume2PartialFill = " + AutoAddVolume2PartialFill);
            builder.AppendLine("AutoUpdate2TradeEvent = " + AutoUpdate2TradeEvent);
            builder.AppendLine("MaxPriceRandomDiff = " + MaxPriceRandomDiff);
            return builder.ToString();
        }

        static public string SymbolNameConvertor(string lpSymbolName)
        {
            return lpSymbolName.Replace(@"/", "");
        }
    }
}
