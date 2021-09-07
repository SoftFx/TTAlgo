using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using System;

namespace TickTrader.Algo.Rpc
{
    public partial class RpcMessage
    {
        public static string GenerateCallId() => Guid.NewGuid().ToString("N");


        public static RpcMessage Request(IMessage payload) => Request(string.Empty, GenerateCallId(), Any.Pack(payload));

        public static RpcMessage Request(string proxyId, IMessage payload) => Request(proxyId, GenerateCallId(), Any.Pack(payload));

        public static RpcMessage Request(string proxyId, string callId, IMessage payload) => Request(proxyId, callId, Any.Pack(payload));

        public static RpcMessage Request(Any payload) => Request(string.Empty, GenerateCallId(), payload);

        public static RpcMessage Request(string proxyId, Any payload) => Request(proxyId, GenerateCallId(), payload);

        public static RpcMessage Request(string proxyId, string callId, Any payload) => GetRpcMessage(callId, RpcFlags.Request, payload, proxyId);


        public static RpcMessage Response(string requestCallId, IMessage payload) => Response(requestCallId, string.Empty, Any.Pack(payload));

        public static RpcMessage Response(string requestCallId, string proxyId, IMessage payload) => Response(requestCallId, proxyId, Any.Pack(payload));

        public static RpcMessage Response(string requestCallId, Any payload) => Response(requestCallId, string.Empty, payload);

        public static RpcMessage Response(string requestCallId, string proxyId, Any payload) => GetRpcMessage(requestCallId, RpcFlags.Response, payload, proxyId);


        public static RpcMessage Notification(IMessage payload) => Notification(string.Empty, string.Empty, Any.Pack(payload));

        public static RpcMessage Notification(string proxyId, IMessage payload) => Notification(proxyId, string.Empty, Any.Pack(payload));

        public static RpcMessage Notification(string proxyId, string callId, IMessage payload) => Notification(proxyId, callId, Any.Pack(payload));

        public static RpcMessage Notification(Any payload) => Notification(string.Empty, string.Empty, payload);

        public static RpcMessage Notification(string proxyId, Any payload) => Notification(proxyId, string.Empty, payload);
        public static RpcMessage Notification(string proxyId, string callId, Any payload) => GetRpcMessage(callId, RpcFlags.Notification, payload, proxyId);


        private static RpcMessage GetRpcMessage(string callId, RpcFlags flag, Any payload, string proxyId) => new RpcMessage { CallId = callId, Flags = flag, Payload = payload, ProxyId = proxyId };
    }
}
