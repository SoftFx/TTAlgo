using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using System;

namespace TickTrader.Algo.Rpc
{
    public partial class RpcMessage
    {
        public static string GenerateCallId() => Guid.NewGuid().ToString("N");

        public static RpcMessage Request(IMessage payload)
        {
            return new RpcMessage { CallId = GenerateCallId(), Flags = RpcFlags.Request, Payload = Any.Pack(payload) };
        }

        public static RpcMessage Request(Any payload)
        {
            return new RpcMessage { CallId = GenerateCallId(), Flags = RpcFlags.Request, Payload = payload };
        }

        public static RpcMessage Request(IMessage payload, string callId)
        {
            return new RpcMessage { CallId = callId, Flags = RpcFlags.Request, Payload = Any.Pack(payload) };
        }

        public static RpcMessage Request(Any payload, string callId)
        {
            return new RpcMessage { CallId = callId, Flags = RpcFlags.Request, Payload = payload };
        }

        public static RpcMessage Response(string requestCallId, IMessage payload)
        {
            return new RpcMessage { CallId = requestCallId, Flags = RpcFlags.Response, Payload = Any.Pack(payload) };
        }

        public static RpcMessage Response(string requestCallId, Any payload)
        {
            return new RpcMessage { CallId = requestCallId, Flags = RpcFlags.Response, Payload = payload };
        }

        public static RpcMessage Notification(IMessage payload)
        {
            return new RpcMessage { CallId = string.Empty, Flags = RpcFlags.Notification, Payload = Any.Pack(payload) };
        }

        public static RpcMessage Notification(Any payload)
        {
            return new RpcMessage { CallId = string.Empty, Flags = RpcFlags.Notification, Payload = payload };
        }

        public static RpcMessage Notification(IMessage payload, string callId)
        {
            return new RpcMessage { CallId = callId, Flags = RpcFlags.Notification, Payload = Any.Pack(payload) };
        }

        public static RpcMessage Notification(Any payload, string callId)
        {
            return new RpcMessage { CallId = callId, Flags = RpcFlags.Notification, Payload = payload };
        }
    }
}
