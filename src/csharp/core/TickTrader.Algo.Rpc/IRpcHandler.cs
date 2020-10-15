using Google.Protobuf.WellKnownTypes;
using System.Threading.Tasks;

namespace TickTrader.Algo.Rpc
{
    public interface IRpcHandler
    {
        void SetSession(RpcSession session);

        Task<Any> HandleRequest(string callId, Any payload);

        void HandleNotification(string callId, Any payload);
    }

    public interface IRpcHost
    {
        ProtocolSpec Resolve(ProtocolSpec protocol, out string error);

        IRpcHandler GetRpcHandler(ProtocolSpec protocol);
    }
}
