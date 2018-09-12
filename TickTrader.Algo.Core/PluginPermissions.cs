using System;
using System.Runtime.Serialization;
using System.Text;

namespace TickTrader.Algo.Core
{
    [Serializable]
    [DataContract(Namespace = "")]
    public class PluginPermissions : ITradePermissions
    {
        [DataMember(Name = "tradeAllowed")]
        public bool TradeAllowed { get; set; }

        public PluginPermissions()
        {
            TradeAllowed = true;
        }

        public override string ToString()
        {
            var res = new StringBuilder();
            res.AppendLine($"Trade Allowed: {TradeAllowed}");
            return res.ToString();
        }
    }
}
