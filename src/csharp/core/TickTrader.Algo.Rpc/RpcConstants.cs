using System;

namespace TickTrader.Algo.Rpc
{
    public static class RpcConstants
    {
        public const int HeartbeatCntThreshold = 3;
        public const string SystemUri = "system";

        public static readonly TimeSpan ConnectTimeout = TimeSpan.FromSeconds(10);
        public static readonly TimeSpan HeartbeatTimeout = TimeSpan.FromSeconds(10);
        //public static readonly TimeSpan HeartbeatTimeout = TimeSpan.FromSeconds(5000); // debug
    }
}