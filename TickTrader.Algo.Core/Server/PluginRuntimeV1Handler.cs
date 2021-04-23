using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Rpc;

namespace TickTrader.Algo.Core
{
    internal class PluginRuntimeV1Handler : IRpcHandler
    {
        private static readonly Any VoidResponse = Any.Pack(new VoidResponse());

        private readonly IRuntimeProxy _runtime;
        private readonly Dictionary<string, IRpcHandler> _knownProxies;
        private RpcSession _session;
        private TaskCompletionSource<bool> _disconnectedTask;
        private IDisposable _rpcStateSub;


        public PluginRuntimeV1Handler(IRuntimeProxy runtime)
        {
            _runtime = runtime;
            _knownProxies = new Dictionary<string, IRpcHandler>();
            _disconnectedTask = new TaskCompletionSource<bool>();
        }


        public Task<bool> AttachRuntime(string runtimeId)
        {
            var context = new RpcResponseTaskContext<bool>(AttachRuntimeResponseHandler);
            _session.Ask(RpcMessage.Request(new AttachRuntimeRequest { Id = runtimeId }), context);
            return context.TaskSrc.Task;
        }

        public Task<RuntimeConfig> GetRuntimeConfig()
        {
            var context = new RpcResponseTaskContext<RuntimeConfig>(RpcHandler.SingleReponseHandler);
            _session.Ask(RpcMessage.Request(new RuntimeConfigRequest()), context);
            return context.TaskSrc.Task;
        }

        public async Task<string> GetPackagePath(string name, int location)
        {
            var request = new PackagePathRequest { Name = name, Location = location };
            var context = new RpcResponseTaskContext<PackagePathResponse>(RpcHandler.SingleReponseHandler);
            _session.Ask(RpcMessage.Request(request), context);
            return (await context.TaskSrc.Task).Path;
        }

        public Task<ExecutorConfig> GetExecutorConfig(string executorId)
        {
            var context = new RpcResponseTaskContext<ExecutorConfig>(RpcHandler.SingleReponseHandler);
            _session.Ask(RpcMessage.Request(executorId, new ExecutorConfigRequest()), context);
            return context.TaskSrc.Task;
        }

        public async Task<IAccountProxy> AttachAccount(string accountId)
        {
            var context = new RpcResponseTaskContext<VoidResponse>(RpcHandler.SingleReponseHandler);
            _session.Ask(RpcMessage.Request(new AttachAccountRequest { AccountId = accountId }), context);
            await context.TaskSrc.Task;

            RemoteAccountProxy account;
            if (_knownProxies.TryGetValue(accountId, out var proxy))
            {
                account = (RemoteAccountProxy)proxy;
            }
            else
            {
                account = new RemoteAccountProxy(accountId);
                _knownProxies.Add(accountId, account);
                (account as IRpcHandler).SetSession(_session);
            }

            await account.AddRef();
            return account;
        }

        public async Task DetachAccount(string accountId)
        {
            if (!_knownProxies.TryGetValue(accountId, out var proxy))
                throw new ArgumentException("Unknown account id");

            var context = new RpcResponseTaskContext<VoidResponse>(RpcHandler.SingleReponseHandler);
            _session.Ask(RpcMessage.Request(new DetachAccountRequest { AccountId = accountId }), context);
            await context.TaskSrc.Task;

            var account = (RemoteAccountProxy)proxy;
            await account.RemoveRef();
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
            _rpcStateSub = session.ObserveStates.Subscribe(OnStateChange);
        }

        public void HandleNotification(string proxyId, string callId, Any payload)
        {
            if (_knownProxies.TryGetValue(proxyId, out var proxy))
                proxy.HandleNotification(proxyId, callId, payload);
        }

        public Task<Any> HandleRequest(string proxyId, string callId, Any payload)
        {
            if (payload.Is(StartRuntimeRequest.Descriptor))
                return StartRuntimeRequestHandler();
            else if (payload.Is(StopRuntimeRequest.Descriptor))
                return StopRuntimeRequestHandler();
            else if (payload.Is(StartExecutorRequest.Descriptor))
                return StartExecutorRequestHandler(payload);
            else if (payload.Is(StopExecutorRequest.Descriptor))
                return StopExecutorRequestHandler(payload);
            else if (payload.Is(PackageInfoRequest.Descriptor))
                return PackageInfoRequestHandler();
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


        private async Task<Any> StartRuntimeRequestHandler()
        {
            await _runtime.Launch();
            return VoidResponse;
        }

        private async Task<Any> StopRuntimeRequestHandler()
        {
            await _runtime.Stop();
            return VoidResponse;
        }

        private async Task<Any> StartExecutorRequestHandler(Any payload)
        {
            var request = payload.Unpack<StartExecutorRequest>();
            await _runtime.StartExecutor(request.ExecutorId);
            return VoidResponse;
        }

        private async Task<Any> StopExecutorRequestHandler(Any payload)
        {
            var request = payload.Unpack<StopExecutorRequest>();
            await _runtime.StopExecutor(request.ExecutorId);
            return VoidResponse;
        }

        private async Task<Any> PackageInfoRequestHandler()
        {
            var info = await _runtime.GetPackageInfo();
            return Any.Pack(info);
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
