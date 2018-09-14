using System;
using System.Runtime.Serialization;
using System.Text;

namespace TickTrader.Algo.Core
{
    [Serializable]
    [DataContract(Namespace = "")]
    public class PluginPermissions : IPluginPermissions
    {
        [DataMember(Name = "tradeAllowed")]
        public bool TradeAllowed { get; set; }


        [DataMember(Name = "isolated")]
        public bool Isolated { get; set; }


        public PluginPermissions()
        {
            TradeAllowed = true;
            Isolated = true;
        }

        public override string ToString()
        {
            var res = new StringBuilder();
            res.AppendLine($"Trade Allowed: {TradeAllowed}");
            res.AppendLine($"Isolated: {Isolated}");
            return res.ToString();
        }

        public PluginPermissions Clone()
        {
            return new PluginPermissions
            {
                TradeAllowed = TradeAllowed,
                Isolated = Isolated,
            };
        }
    }
}
