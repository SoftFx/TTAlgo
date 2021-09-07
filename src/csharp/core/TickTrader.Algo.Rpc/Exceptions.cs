using System;

namespace TickTrader.Algo.Rpc
{
    public class RpcStateException : Exception
    {
        public RpcStateException(string message) : base(message) { }

        public static RpcStateException AnotherTransportListener()
        {
            return new RpcStateException("Transport already has a listener");
        }
        
        public static RpcStateException NotConnected()
        {
            return new RpcStateException("Session is not connected");
        }

        public static RpcStateException NotDisconnected()
        {
            return new RpcStateException("Session is not disconnected");
        }

        public static RpcStateException DuplicateCallId()
        {
            return new RpcStateException("Duplicate call id");
        }
    }
}
