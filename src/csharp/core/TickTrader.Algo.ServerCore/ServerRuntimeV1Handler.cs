using Google.Protobuf.WellKnownTypes;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using TickTrader.Algo.Async.Actors;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Rpc;

namespace TickTrader.Algo.Server
{
    public class ServerRuntimeV1Handler : IRpcHandler
    {
        private readonly ConcurrentDictionary<string, AccountRpcHandler> _accProxies = new ConcurrentDictionary<string, AccountRpcHandler>();

        private readonly IActorRef _host;
        private IActorRef _runtime;
        private RpcSession _session;


        public ServerRuntimeV1Handler(IActorRef host)
        {
            _host = host;
        }


        public void SetSession(RpcSession session)
        {
            _session = session;
        }

        public void HandleNotification(string proxyId, string callId, Any payload)
        {
            RuntimeControlModel.OnExecutorNotification(_runtime, proxyId, payload);
        }

        public Task<Any> HandleRequest(string proxyId, string callId, Any payload)
        {
            if (payload.Is(AttachRuntimeRequest.Descriptor))
                return AttachRuntimeRequestHandler(payload);
            else if (payload.Is(AttachAccountRequest.Descriptor))
                return AttachAccountRequestHandler(payload);
            else if (payload.Is(DetachAccountRequest.Descriptor))
                return DetachAccountRequestHandler(payload);
            else if (_accProxies.TryGetValue(proxyId, out var acc))
                return acc.HandleRequest(callId, payload);

            return string.IsNullOrEmpty(proxyId)
                ? Task.FromResult(default(Any))
                : Task.FromResult(Any.Pack(new ErrorResponse { Message = $"Unknown proxy id '{proxyId}'" }));
        }


        private async Task<Any> AttachRuntimeRequestHandler(Any payload)
        {
            var request = payload.Unpack<AttachRuntimeRequest>();
            if (_runtime != null)
                return Any.Pack(new ErrorResponse { Message = "Runtime already attached!" });

            _runtime = await RuntimeServerModel.GetRuntime(_host, request.Id);
            var success = false;
            if (_runtime != null)
                success = await RuntimeControlModel.ConnectSession(_runtime, _session);

            return Any.Pack(new AttachRuntimeResponse { Success = success });
        }

        private async Task<Any> AttachAccountRequestHandler(Any payload)
        {
            var request = payload.Unpack<AttachAccountRequest>();

            var accId = request.AccountId;
            var accControl = await RuntimeServerModel.GetAccountControl(_host, accId);
            var handler = await AccountRpcModel.AttachSession(accControl, _session);
            _accProxies.TryAdd(accId, handler);

            return RpcHandler.VoidResponse;
        }

        private async Task<Any> DetachAccountRequestHandler(Any payload)
        {
            var request = payload.Unpack<DetachAccountRequest>();

            var accId = request.AccountId;
            _accProxies.TryRemove(accId, out var _);
            var accControl = await RuntimeServerModel.GetAccountControl(_host, accId);
            await AccountRpcModel.DetachSession(accControl, _session.Id);

            return RpcHandler.VoidResponse;
        }
    }
}
