﻿using System.Runtime.Serialization;

namespace TickTrader.Algo.Core
{
    [DataContract(Namespace = "")]
    public class PluginPermissions : ITradePermissions
    {
        [DataMember(Name = "tradeAllowed")]
        public bool TradeAllowed { get; set; }
    }
}
