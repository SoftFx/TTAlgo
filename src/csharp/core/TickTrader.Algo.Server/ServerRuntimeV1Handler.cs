using Google.Protobuf.WellKnownTypes;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Rpc;

namespace TickTrader.Algo.Server
{
    internal class ServerRuntimeV1Handler : IRpcHandler
    {
        private readonly ConcurrentDictionary<string, AccountRpcHandler> _accProxies = new ConcurrentDictionary<string, AccountRpcHandler>();

        private readonly AlgoServer _server;
        private PkgRuntimeModel _runtime;
        private RpcSession _session;



        public ServerRuntimeV1Handler(AlgoServer server)
        {
            _server = server;
        }


        public void SetSession(RpcSession session)
        {
            _session = session;
        }

        public void HandleNotification(string proxyId, string callId, Any payload)
        {
            _runtime.OnExecutorNotification(proxyId, payload);

        }

        public Task<Any> HandleRequest(string proxyId, string callId, Any payload)
        {
            if (payload.Is(AttachRuntimeRequest.Descriptor))
                return AttachRuntimeRequestHandler(payload);
            else if (payload.Is(RuntimeConfigRequest.Descriptor))
                return RuntimeConfigRequestHandler();
            else if (payload.Is(ExecutorConfigRequest.Descriptor))
                return ExecutorConfigRequestHandler(proxyId);
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

            _runtime = await _server.Runtimes.ConnectRuntime(request.Id, _session);
            return Any.Pack(new AttachRuntimeResponse { Success = _runtime != null });
        }

        private Task<Any> RuntimeConfigRequestHandler()
        {
            return _runtime.GetConfig().ContinueWith(t => Any.Pack(t.Result));
        }

        private Task<Any> ExecutorConfigRequestHandler(string executorId)
        {
            return _runtime.GetExecutorConfig(executorId).ContinueWith(t => Any.Pack(t.Result));
        }

        private async Task<Any> AttachAccountRequestHandler(Any payload)
        {
            var request = payload.Unpack<AttachAccountRequest>();

            var accId = request.AccountId;
            var accControl = await _server.Accounts.GetAccountControl(accId);
            await accControl.AttachSession(_session);
            if (!_accProxies.ContainsKey(accId))
            {
                var accProxy = new AccountRpcHandler(await accControl.GetAccountProxy(), _session);
                _accProxies[accId] = accProxy;
            }

            return RpcHandler.VoidResponse;
        }

        private async Task<Any> DetachAccountRequestHandler(Any payload)
        {
            var request = payload.Unpack<DetachAccountRequest>();

            var accId = request.AccountId;
            var accControl = await _server.Accounts.GetAccountControl(accId);
            await accControl.DetachSession(_session.Id);

            return RpcHandler.VoidResponse;
        }
    }
}
