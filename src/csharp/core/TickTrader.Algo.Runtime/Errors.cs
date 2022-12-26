using System;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Runtime
{
    internal static class Errors
    {
        public static Exception RuntimeNotStarted(string runtimeId) => new AlgoException($"Runtime '{runtimeId}' is not started");
    }
}
