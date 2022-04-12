using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using TickTrader.Algo.Async.Actors;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Rpc;

namespace TickTrader.Algo.BacktesterApi
{
    internal class BacktesterControlActor : Actor
    {
        public const int KillTimeout = 20000;

        private readonly RpcProxyParams _rpcParams;
        private readonly string _exePath;
        private readonly string _resultsDir;
        private readonly IActorRef _parent;
        private readonly List<TaskCompletionSource<object>> _awaitStopList = new List<TaskCompletionSource<object>>();
        private readonly ActorEventSource<BacktesterProgressUpdate> _progressEventSrc = new ActorEventSource<BacktesterProgressUpdate>();
        private readonly ActorEventSource<BacktesterStateUpdate> _stateEventSrc = new ActorEventSource<BacktesterStateUpdate>();

        private IAlgoLogger _logger;
        private RpcSession _session;
        private TaskCompletionSource<object> _initTaskSrc;
        private Process _process;
        private CancellationTokenSource _killProcessCancelSrc;
        private bool _disposedNormal, _disposedExplicit, _killedByTimeout;


        private bool Disposed => _disposedNormal || _disposedExplicit;


        private BacktesterControlActor(RpcProxyParams rpcParams, string exePath, string resultsDir, IActorRef parent)
        {
            _rpcParams = rpcParams;
            _exePath = exePath;
            _resultsDir = resultsDir;
            _parent = parent;

            Receive<InitCmd>(Init);
            Receive<DisposeCmd>(Dispose);
            Receive<ConnectSessionCmd, bool>(ConnectSession);
            Receive<ProcessExitedMsg>(OnProcessExited);
            Receive<StartBacktesterRequest>(Start);
            Receive<StopBacktesterRequest>(Stop);
            Receive<BacktesterController.AwaitStopRequest>(AwaitStop);
            Receive<BacktesterStoppedMsg>(OnStopped);
            Receive<BacktesterController.SubscribeToProgressUpdatesCmd>(SubscribeToProgressUpdates);
            Receive<BacktesterProgressUpdate>(OnProgressUpdate);
            Receive<BacktesterController.SubscribeToStateUpdatesCmd>(SubscribeToStateUpdate);
            Receive<BacktesterStateUpdate>(OnStateUpdate);
        }


        public static IActorRef Create(RpcProxyParams rpcParams, string exePath, string resultsDir, IActorRef parent)
        {
            return ActorSystem.SpawnLocal(() => new BacktesterControlActor(rpcParams, exePath, resultsDir, parent), $"{nameof(BacktesterControlActor)} ({rpcParams.ProxyId})");
        }


        protected override void ActorInit(object initMsg)
        {
            _logger = AlgoLoggerFactory.GetLogger(Name);
        }


        private Task Init(InitCmd cmd)
        {
            _initTaskSrc = new TaskCompletionSource<object>();
            StartProcess();
            return _initTaskSrc.Task;
        }

        private void Dispose(DisposeCmd cmd)
        {
            if (Disposed)
                return;

            _disposedExplicit = true;
            _session?.Disconnect("Backtester dispose");
            ScheduleKillProcess();
        }

        private bool ConnectSession(ConnectSessionCmd cmd)
        {
            if (_session != null)
            {
                _initTaskSrc?.TrySetException(new System.Exception("Session already attached"));
                _initTaskSrc = null;
                return false;
            }

            _session = cmd.Session;
            _initTaskSrc?.TrySetResult(null);
            _initTaskSrc = null;
            return true;
        }

        private async Task Start(StartBacktesterRequest request)
        {
            if (Disposed)
                throw new ObjectDisposedException(Name);

            var context = new RpcResponseTaskContext<VoidResponse>(RpcHandler.SingleReponseHandler);
            _session.Ask(RpcMessage.Request(request), context);
            await context.TaskSrc.Task;
        }

        private Task Stop(StopBacktesterRequest request)
        {
            if (Disposed)
                return Task.CompletedTask;

            var context = new RpcResponseTaskContext<VoidResponse>(RpcHandler.SingleReponseHandler);
            _session.Ask(RpcMessage.Request(request), context);
            return context.TaskSrc.Task;
        }

        private Task AwaitStop(BacktesterController.AwaitStopRequest request)
        {
            if (Disposed)
                return Task.CompletedTask;

            var taskSrc = new TaskCompletionSource<object>();
            _awaitStopList.Add(taskSrc);
            return taskSrc.Task;
        }

        private void OnStopped(BacktesterStoppedMsg msg)
        {
            if (Disposed)
                return;

            _disposedNormal = true;
            _session?.Disconnect("Backtester shutdown");
            ScheduleKillProcess();
        }

        private void SubscribeToProgressUpdates(BacktesterController.SubscribeToProgressUpdatesCmd cmd)
        {
            _progressEventSrc.Subscribe(cmd.Sink);
        }

        private void OnProgressUpdate(BacktesterProgressUpdate msg)
        {
            _progressEventSrc.DispatchEvent(msg);
        }

        private void SubscribeToStateUpdate(BacktesterController.SubscribeToStateUpdatesCmd cmd)
        {
            _stateEventSrc.Subscribe(cmd.Sink);
        }

        private void OnStateUpdate(BacktesterStateUpdate msg)
        {
            _stateEventSrc.DispatchEvent(msg);
        }


        private bool StartProcess()
        {
            var startInfo = new ProcessStartInfo(_exePath)
            {
                UseShellExecute = false,
                WorkingDirectory = _resultsDir,
                CreateNoWindow = true,
            };
            _rpcParams.SaveAsEnvVars(startInfo.Environment);

            _process = Process.Start(startInfo);
            _process.Exited += OnProcessExit;
            _process.EnableRaisingEvents = true;

            _logger.Info($"Backtester host started in process {_process.Id}");

            if (_process.HasExited) // If event was enabled after actual stop
            {
                OnProcessExit(_process, null);
                return false;
            }

            return true;
        }

        private void ScheduleKillProcess()
        {
            if (_killProcessCancelSrc != null)
                return;

            _killProcessCancelSrc = new CancellationTokenSource();
            TaskExt.Schedule(KillTimeout, () =>
            {
                _killedByTimeout = true;
                _logger.Info($"Backtester host didn't stop within timeout. Killing process {_process.Id}...");
                _process.Kill();
            }, _killProcessCancelSrc.Token, Scheduler);
        }

        private void OnProcessExited(ProcessExitedMsg msg)
        {
            _process.Exited -= OnProcessExit;
            _killProcessCancelSrc?.Cancel();
            if (_initTaskSrc != null)
            {
                _initTaskSrc.TrySetException(new Exception("Backtester process failed to start"));
            }
            else if (msg.ExitCode != 0)
            {
                try
                {
                    var status = BacktesterResults.Internal.TryReadExecStatus(_resultsDir) ?? new ExecutionStatus();
                    status.SetError(_killedByTimeout
                        ? $"Backtester process failed to stop within timeout, exit code {msg.ExitCode}"
                        : $"Backtester process failed with exit code {msg.ExitCode}");
                    BacktesterResults.Internal.SaveExecStatus(_resultsDir, status);
                }
                catch(Exception ex)
                {
                    _logger.Error(ex, "Failed to update backtester execution status");
                }
            }

            Exception error = null;
            try
            {
                BacktesterResults.Internal.CompressResultsDir(_resultsDir);
            }
            catch (Exception ex) { error = ex; }

            foreach (var awaiter in _awaitStopList)
            {
                if (error != null)
                    awaiter.TrySetException(error);
                else
                    awaiter.TrySetResult(null);
            }

            _awaitStopList.Clear();

            _parent.Tell(new InstanceShutdownMsg(_rpcParams.ProxyId));
        }

        private void OnProcessExit(object sender, EventArgs args)
        {
            // runs directly on thread pool
            try
            {
                // non-actor context
                var exitCode = _process.ExitCode;
                _logger.Info($"Process {_process.Id} exited with exit code {exitCode}");
                Self.Tell(new ProcessExitedMsg(exitCode));
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Process exit handler failed");
            }
        }


        public class InitCmd : Singleton<InitCmd> { }

        public class DisposeCmd : Singleton<DisposeCmd> { }

        public class ConnectSessionCmd
        {
            public RpcSession Session { get; }

            public ConnectSessionCmd(RpcSession session)
            {
                Session = session;
            }
        }

        internal class ProcessExitedMsg
        {
            public int ExitCode { get; }

            public ProcessExitedMsg(int exitCode)
            {
                ExitCode = exitCode;
            }
        }

        public class InstanceShutdownMsg
        {
            public string Id { get; }

            public InstanceShutdownMsg(string id)
            {
                Id = id;
            }
        }
    }
}
