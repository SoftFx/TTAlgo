using System;
using System.Collections.Generic;
using TickTrader.Algo.Api;
using System.Linq;

namespace TickTrader.Algo.Core
{
    [Serializable]
    public class AccountEntity
    {
        public string Id { get; set; }
        public double Balance { get; set; }
        public string BalanceCurrency { get; set; }
        public int Leverage { get; set; }
        public AccountTypes Type { get; set; }
        public AssetEntity[] Assets { get; set; }
    }
}

