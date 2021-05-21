using System;
using System.Collections.Generic;
using System.Linq;

namespace TickTrader.Algo.Core.Lib
{
    public static class EnumHelper
    {
        public static IEnumerable<TEnum> AllValues<TEnum>() where TEnum : struct, IConvertible
        {
            return Enum.GetValues(typeof(TEnum)).Cast<TEnum>();
        }

    }
}
