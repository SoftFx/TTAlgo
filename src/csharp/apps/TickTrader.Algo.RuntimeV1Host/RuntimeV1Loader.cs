using System;
using System.Diagnostics;
using System.Threading.Tasks;
using TickTrader.Algo.Async.Actors;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Rpc;
using TickTrader.Algo.Rpc.OverTcp;
using TickTrader.Algo.Runtime;

namespace TickTrader.Algo.RuntimeV1Host
{
    public class RuntimeV1Loader : IRpcHost, IRuntimeProxy
    {
        public const int AbortTimeout = 10000;

        private static readonly ProtocolSpec ExpectedProtocol = new ProtocolSpec { Url = KnownProtocolUrls.RuntimeV1, MajorVerion = 1, MinorVerion = 0 };

        private readonly RpcClient _client;
        private readonly PluginRuntimeV1Handler _handler;

        private IAlgoLogger _logger;
        private string _id;
        private IActorRef _runtime;
        private Process _parentProc;
        private TaskCompletionSource<object> _finishedTaskSrc;


        public RuntimeV1Loader()
        {
            _client = new RpcClient(new TcpFactory(), this, ExpectedProtocol);
            _handler = new PluginRuntimeV1Handler(this);
        }


        public async Task Init(string address, int port, string proxyId, int? parentPid)
        {
            _logger = AlgoLoggerFactory.GetLogger<RuntimeV1Loader>();
            _id = proxyId;

            _finishedTaskSrc = new TaskCompletionSource<object>();

            if (parentPid.HasValue)
            {
                _parentProc = Process.GetProcessById(parentPid.Value);
                _parentProc.Exited += OnProcessExited;
                _parentProc.EnableRaisingEvents = true;
                if (_parentProc.HasExited)
                {
                    _logger.Error("Parent process already stopped");
                    OnProcessExited(_parentProc, null);
                    return;
                }
            }

            await _client.Connect(address, port).ConfigureAwait(false);
            var attached = await _handler.AttachRuntime(proxyId).ConfigureAwait(false);
            if (!attached)
            {
                _logger.Error("Runtime was not attached");
                await _client.Disconnect("Runtime shutdown").ConfigureAwait(false);
            }
        }

        public async Task Deinit()
        {
            await _client.Disconnect("Runtime shutdown").ConfigureAwait(false);
        }

        public Task WhenFinished()
        {
            _handler.WhenDisconnected().ContinueWith(_ => _finishedTaskSrc.TrySetResult(null));

            return _finishedTaskSrc.Task;
        }

        public async Task Start(StartRuntimeRequest request)
        {
            _runtime = RuntimeV1Actor.Create(_id, _handler);

            await _runtime.Ask(request);
        }

        public Task Stop(StopRuntimeRequest request)
        {
            return _runtime.Ask(request);
        }

        public Task StartExecutor(StartExecutorRequest request)
        {
            return _runtime.Ask(request);
        }

        public Task StopExecutor(StopExecutorRequest request)
        {
            return _runtime.Ask(request);
        }


        #region IRpcHost implementation

        ProtocolSpec IRpcHost.Resolve(ProtocolSpec protocol, out string error)
        {
            error = string.Empty;
            return protocol;
        }

        IRpcHandler IRpcHost.GetRpcHandler(ProtocolSpec protocol)
        {
            return ExpectedProtocol.Url == protocol.Url ? _handler : null;
        }

        #endregion IRpcHost implementation


        private void OnProcessExited(object sender, EventArgs args)
        {
            _parentProc.Exited -= OnProcessExited;

            _logger.Info($"Parent process {_parentProc.Id} exited with code {_parentProc.ExitCode}");

            Task.Delay(1000).ContinueWith(_ => _finishedTaskSrc.TrySetResult(null));
        }
    }
}
