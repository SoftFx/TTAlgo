using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMBot2
{
    public class MMBotTOMLConfiguration
    {
        Dictionary<string, double> syntetics = new Dictionary<string, double>();
        public Dictionary<string, double> Syntetics
        {
            get
            {
                return syntetics;
            }
            set
            {
                syntetics = value;
                ParsedSyntetics.Clear();
                foreach (KeyValuePair<string, double> currPair in syntetics)
                {
                    string[] currencies = currPair.Key.Split(new char[] { '-' });
                    ParsedSyntetics.Add(currencies, currPair.Value);
                }
            }
        }
        public double MarkupInPercent { get; set; }
        public Dictionary<string[], double> ParsedSyntetics = new Dictionary<string[], double>();

        public MMBotTOMLConfiguration()
        {
            //Dictionary<string, double>  t = new Dictionary<string, double>();
            //t.Add("BTC-USD-EUR", 2);
            //Syntetics = t;
            //MarkupInPercent = 1;
        }
    }
}
