using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using System;

namespace TickTrader.Algo.Rpc
{
    public partial class RpcMessage
    {
        public static string GenerateCallId() => Guid.NewGuid().ToString("N");


        public static RpcMessage Request(IMessage payload) => Request(Any.Pack(payload));

        public static RpcMessage Request(IMessage payload, string callId) => Request(Any.Pack(payload), callId);

        public static RpcMessage Request(Any payload) => GetRpcMessage(GenerateCallId(), RpcFlags.Request, payload);

        public static RpcMessage Request(Any payload, string callId) => GetRpcMessage(callId, RpcFlags.Request, payload);


        public static RpcMessage Response(string requestCallId, IMessage payload) => Response(requestCallId, Any.Pack(payload));

        public static RpcMessage Response(string requestCallId, Any payload) => GetRpcMessage(requestCallId, RpcFlags.Response, payload);


        public static RpcMessage Notification(Any payload) => Notification(payload, string.Empty);

        public static RpcMessage Notification(IMessage payload) => Notification(Any.Pack(payload), string.Empty);

        public static RpcMessage Notification(IMessage payload, string callId) => Notification(Any.Pack(payload), callId);

        public static RpcMessage Notification(Any payload, string callId) => GetRpcMessage(callId, RpcFlags.Notification, payload);


        private static RpcMessage GetRpcMessage(string callId, RpcFlags flag, Any payload) => new RpcMessage { CallId = callId, Flags = flag, Payload = payload };
    }
}
