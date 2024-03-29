﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Api
{
    public interface IBalanceDividendEventArgs
    {
        double Balance { get; set; }

        string Currency { get; set; }

        double TransactionAmount { get; set; }
    }
}
