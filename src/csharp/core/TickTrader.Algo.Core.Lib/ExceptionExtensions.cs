using System;

namespace TickTrader.Algo.Core.Lib
{
    public static class ExceptionExtensions
    {
        public static Exception FlattenAsPossible(this Exception ex)
        {
            var aggrEx = ex as AggregateException;
            if (aggrEx != null && aggrEx.InnerExceptions.Count == 1)
                return FlattenAsPossible(aggrEx.InnerExceptions[0]);
            else
                return ex;
        }
    }
}
