using Google.Protobuf.WellKnownTypes;
using System;

namespace TickTrader.Algo.Core
{
    public static class ToAlgo
    {
        public static Timestamp ToTimestamp(this DateTime dateTime)
        {
            return Timestamp.FromDateTime(dateTime);
        }

        public static Timestamp ToTimestamp(this DateTime? dateTime)
        {
            return dateTime.HasValue ? Timestamp.FromDateTime(dateTime.Value) : null;
        }
    }
}
