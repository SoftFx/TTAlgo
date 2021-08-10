using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using TickTrader.Algo.Core;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Rpc;

namespace TickTrader.Algo.Runtime
{
    internal class PluginRuntimeV1Handler : IRpcHandler
    {
        private readonly IRuntimeProxy _runtime;
        private readonly ConcurrentDictionary<string, IRpcHandler> _knownProxies;
        private RpcSession _session;
        private TaskCompletionSource<bool> _disconnectedTask;
        private IDisposable _rpcStateSub;


        public PluginRuntimeV1Handler(IRuntimeProxy runtime)
        {
            _runtime = runtime;
            _knownProxies = new ConcurrentDictionary<string, IRpcHandler>();
            _disconnectedTask = new TaskCompletionSource<bool>();
        }


        public Task<bool> AttachRuntime(string runtimeId)
        {
            var context = new RpcResponseTaskContext<bool>(AttachRuntimeResponseHandler);
            _session.Ask(RpcMessage.Request(new AttachRuntimeRequest { Id = runtimeId }), context);
            return context.TaskSrc.Task;
        }

        public async Task AttachAccount(string accountId, IRpcHandler handler)
        {
            if (_knownProxies.ContainsKey(accountId))
                return;

            _knownProxies.TryAdd(accountId, handler);
            handler.SetSession(_session);

            try
            {
                var context = new RpcResponseTaskContext<VoidResponse>(RpcHandler.SingleReponseHandler);
                _session.Ask(RpcMessage.Request(new AttachAccountRequest { AccountId = accountId }), context);
                await context.TaskSrc.Task;
            }
            catch(Exception)
            {
                handler.SetSession(null);
                _knownProxies.TryRemove(accountId, out var _);

                throw;
            }
        }

        public async Task DetachAccount(string accountId)
        {
            if (!_knownProxies.ContainsKey(accountId))
                throw new ArgumentException("Unknown account id");

            _knownProxies.TryRemove(accountId, out var _);

            var context = new RpcResponseTaskContext<VoidResponse>(RpcHandler.SingleReponseHandler);
            _session.Ask(RpcMessage.Request(new DetachAccountRequest { AccountId = accountId }), context);
            await context.TaskSrc.Task;
        }

        internal void SendNotification(string proxyId, IMessage msg)
        {
            _session.Tell(RpcMessage.Notification(proxyId, msg));
        }

        public Task WhenDisconnected()
        {
            return _disconnectedTask.Task;
        }


        public void SetSession(RpcSession session)
        {
            _session = session;
            foreach (var proxy in _knownProxies.Values)
            {
                proxy.SetSession(_session);
            }
            _rpcStateSub = session.StateChanged.Subscribe(OnStateChange);
        }

        public void HandleNotification(string proxyId, string callId, Any payload)
        {
            if (_knownProxies.TryGetValue(proxyId, out var proxy))
                proxy.HandleNotification(proxyId, callId, payload);
        }

        public Task<Any> HandleRequest(string proxyId, string callId, Any payload)
        {
            if (payload.Is(StartRuntimeRequest.Descriptor))
                return StartRuntimeRequestHandler(payload);
            else if (payload.Is(StopRuntimeRequest.Descriptor))
                return StopRuntimeRequestHandler(payload);
            else if (payload.Is(StartExecutorRequest.Descriptor))
                return StartExecutorRequestHandler(payload);
            else if (payload.Is(StopExecutorRequest.Descriptor))
                return StopExecutorRequestHandler(payload);
            else if (_knownProxies.TryGetValue(proxyId, out var proxy))
                proxy.HandleRequest(proxyId, callId, payload);

            return Task.FromResult(default(Any));
        }


        private void OnStateChange(RpcSessionStateChangedArgs args)
        {
            if (args.NewState == RpcSessionState.Disconnected)
            {
                _disconnectedTask.TrySetResult(true);
                _rpcStateSub.Dispose();
            }
        }


        private async Task<Any> StartRuntimeRequestHandler(Any payload)
        {
            var request = payload.Unpack<StartRuntimeRequest>();
            await _runtime.Start(request);
            return RpcHandler.VoidResponse;
        }

        private async Task<Any> StopRuntimeRequestHandler(Any payload)
        {
            var request = payload.Unpack<StopRuntimeRequest>();
            await _runtime.Stop(request);
            return RpcHandler.VoidResponse;
        }

        private async Task<Any> StartExecutorRequestHandler(Any payload)
        {
            var request = payload.Unpack<StartExecutorRequest>();
            await _runtime.StartExecutor(request);
            return RpcHandler.VoidResponse;
        }

        private async Task<Any> StopExecutorRequestHandler(Any payload)
        {
            var request = payload.Unpack<StopExecutorRequest>();
            await _runtime.StopExecutor(request);
            return RpcHandler.VoidResponse;
        }

        private bool AttachRuntimeResponseHandler(TaskCompletionSource<bool> taskSrc, Any payload)
        {
            if (payload.Is(ErrorResponse.Descriptor))
            {
                var error = payload.Unpack<ErrorResponse>();
                taskSrc.TrySetException(new Exception(error.Message));
                return true;
            }
            var response = payload.Unpack<AttachRuntimeResponse>();
            taskSrc.TrySetResult(response.Success);
            return true;
        }
    }
}
