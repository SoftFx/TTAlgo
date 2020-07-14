using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.Core
{
    public static class CoreEnumConverterExtentions
    {
        public static Domain.OrderInfo.Types.Side ToCoreEnum(this OrderSide side)
        {
            switch (side)
            {
                case OrderSide.Buy: return Domain.OrderInfo.Types.Side.Buy;
                case OrderSide.Sell: return Domain.OrderInfo.Types.Side.Sell;

                default: throw new ArgumentException($"Unsupported type {side}");
            }
        }
    }
}
