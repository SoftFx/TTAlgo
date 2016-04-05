﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Api
{
    public interface AccountDataProvider
    {
        double Balance { get; }
        OrderList Orders { get; }
        PositionList Positions { get; }
    }
}
