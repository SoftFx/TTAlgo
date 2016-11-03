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
                builder.AppendLine(ConvertToLocalSymbolName(pair.Key) + " = " + pair.Value);
            builder.AppendLine("MarkupInPercent = " + MarkupInPercent);
            builder.AppendLine("DebugStatus = " + DebugStatus);
            builder.AppendLine("BotTag = " + BotTag);
            return builder.ToString();
        }

        static public string ConvertToLocalSymbolName(string lpSymbolName)
        {
            return lpSymbolName.Replace(@"/", "");
        }
    }
}
