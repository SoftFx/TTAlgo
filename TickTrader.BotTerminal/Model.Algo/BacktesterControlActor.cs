using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using TickTrader.Algo.Async.Actors;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Rpc;

namespace TickTrader.BotTerminal
{
    internal class BacktesterControlActor : Actor
    {
        public const int KillTimeout = 2000;

        private readonly string _id;
        private readonly string _exePath;
        private readonly string _address;
        private readonly int _port;

        private IAlgoLogger _logger;
        private RpcSession _session;
        private TaskCompletionSource<object> _initTaskSrc;
        private Process _process;
        private CancellationTokenSource _killProcessCancelSrc;


        private BacktesterControlActor(string id, string exePath, string address, int port)
        {
            _id = id;
            _exePath = exePath;
            _address = address;
            _port = port;

            Receive<InitCmd>(Init);
            Receive<DeinitCmd>(Deinit);
            Receive<ConnectSessionCmd, bool>(ConnectSession);
            Receive<ProcessExitedMsg>(OnProcessExited);
            Receive<StartBacktesterRequest>(Start);
        }


        public static IActorRef Create(string id, string exePath, string address, int port)
        {
            return ActorSystem.SpawnLocal(() => new BacktesterControlActor(id, exePath, address, port), $"{nameof(BacktesterControlActor)} ({id})");
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


        private bool StartProcess()
        {
            var startInfo = new ProcessStartInfo(_exePath)
            {
                UseShellExecute = true,
                Arguments = string.Join(" ", _address, _port.ToString(), $"\"{_id}\""),
                //WorkingDirectory = _server.Env.AppFolder,
            };

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
    }
}
