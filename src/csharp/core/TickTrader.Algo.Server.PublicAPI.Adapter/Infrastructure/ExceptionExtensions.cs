﻿using System;

namespace TickTrader.Algo.Server.PublicAPI.Adapter
{
    public static class ExceptionExtensions
    {
        public static Exception Flatten(this Exception ex)
        {
            var aggregateEx = ex as AggregateException;
            if (aggregateEx != null && aggregateEx.InnerExceptions.Count > 0)
            {
                return aggregateEx.InnerExceptions[0];
            }
            return ex;
        }
    }
}
