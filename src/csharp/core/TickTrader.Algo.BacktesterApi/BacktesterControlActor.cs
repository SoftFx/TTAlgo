using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using TickTrader.Algo.Async.Actors;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Rpc;

namespace TickTrader.Algo.BacktesterApi
{
    internal class BacktesterControlActor : Actor
    {
        public const int KillTimeout = 20000;

        private readonly RpcProxyParams _rpcParams;
        private readonly string _exePath;
        private readonly string _workDir;
        private readonly IActorRef _parent;
        private readonly List<TaskCompletionSource<object>> _awaitStopList = new List<TaskCompletionSource<object>>();
        private readonly ActorEventSource<BacktesterProgressUpdate> _progressEventSrc = new ActorEventSource<BacktesterProgressUpdate>();
        private readonly ActorEventSource<BacktesterStateUpdate> _stateEventSrc = new ActorEventSource<BacktesterStateUpdate>();

        private IAlgoLogger _logger;
        private RpcSession _session;
        private TaskCompletionSource<object> _initTaskSrc;
        private Process _process;
        private CancellationTokenSource _killProcessCancelSrc;


        private BacktesterControlActor(RpcProxyParams rpcParams, string exePath, string workDir, IActorRef parent)
        {
            _rpcParams = rpcParams;
            _exePath = exePath;
            _workDir = workDir;
            _parent = parent;

            Receive<InitCmd>(Init);
            Receive<DeinitCmd>(Deinit);
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


        public static IActorRef Create(RpcProxyParams rpcParams, string exePath, string workDir, IActorRef parent)
        {
            return ActorSystem.SpawnLocal(() => new BacktesterControlActor(rpcParams, exePath, workDir, parent), $"{nameof(BacktesterControlActor)} ({rpcParams.ProxyId})");
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

        private void Deinit(DeinitCmd cmd)
        {
            if (_session != null)
            {
                _session.Disconnect("Backtester deinit");
                ScheduleKillProcess();
            }
        }

        private bool ConnectSession(ConnectSessionCmd cmd)
        {
            if (_session != null)
            {
                _initTaskSrc?.TrySetException(new System.Exception("Session already attached"));
                return false;
            }

            _session = cmd.Session;
            _initTaskSrc?.TrySetResult(null);
            return true;
        }

        private Task Start(StartBacktesterRequest request)
        {
            var context = new RpcResponseTaskContext<VoidResponse>(RpcHandler.SingleReponseHandler);
            _session.Ask(RpcMessage.Request(request), context);
            return context.TaskSrc.Task;
        }

        private Task Stop(StopBacktesterRequest request)
        {
            var context = new RpcResponseTaskContext<VoidResponse>(RpcHandler.SingleReponseHandler);
            _session.Ask(RpcMessage.Request(request), context);
            return context.TaskSrc.Task;
        }

        private Task AwaitStop(BacktesterController.AwaitStopRequest request)
        {
            var taskSrc = new TaskCompletionSource<object>();
            _awaitStopList.Add(taskSrc);
            return taskSrc.Task;
        }

        private void OnStopped(BacktesterStoppedMsg msg)
        {
            var hasError = !string.IsNullOrEmpty(msg.ErrorMsg);
            Exception error = default;
            if (hasError)
                error = new AlgoException(msg.ErrorMsg);

            foreach (var awaiter in _awaitStopList)
            {
                if (hasError)
                    awaiter.TrySetException(error);
                else
                    awaiter.TrySetResult(null);
            }

            _awaitStopList.Clear();
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
                WorkingDirectory = _workDir,
                CreateNoWindow = true,
            };
            _rpcParams.SaveAsEnvVars(startInfo.Environment);

            _process = Process.Start(startInfo);
            _process.Exited += OnProcessExit;
            _process.EnableRaisingEvents = true;

            _logger.Info($"Runtime host started in process {_process.Id}");

            if (_process.HasExited) // If event was enabled after actual stop
            {
                OnProcessExit(_process, null);
                return false;
            }

            return true;
        }

        private void ScheduleKillProcess()
        {
            _killProcessCancelSrc = new CancellationTokenSource();
            TaskExt.Schedule(KillTimeout, () =>
            {
                _logger.Info($"Runtime host didn't stop within timeout. Killing process {_process.Id}...");
                _process.Kill();
            }, _killProcessCancelSrc.Token);
        }

        private void OnProcessExited(ProcessExitedMsg msg)
        {
            _process.Exited -= OnProcessExit;
            _killProcessCancelSrc?.Cancel();
            if (_initTaskSrc != null)
            {
                _initTaskSrc.TrySetException(new Exception("Backtester process failed to start"));
            }
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

        public class DeinitCmd : Singleton<DeinitCmd> { }

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
