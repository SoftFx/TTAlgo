using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Runtime;

namespace TickTrader.Algo.Server
{
    public interface IRuntimeHostProxy
    {
        Task Start(string address, int port, string proxyId);

        Task Stop();
    }


    public static class RuntimeHost
    {
        public static IRuntimeHostProxy Create(bool isolated)
        {
            if (isolated)
                return new ChildProcessRuntimeHost();

            return new TransparentRuntimeHost();
        }
    }


    public class TransparentRuntimeHost : IRuntimeHostProxy
    {
        private RuntimeV1Loader _runtime;


        public async Task Start(string address, int port, string proxyId)
        {
            _runtime = new RuntimeV1Loader();

            await Task.Run(() => _runtime.Init(address, port, proxyId));
        }

        public async Task Stop()
        {
            await Task.Delay(2000); // ugly hack to give code in another domain some time for correct stop without thread abort

            await Task.Run(() => _runtime.Deinit());
        }
    }


    public class ChildProcessRuntimeHost : IRuntimeHostProxy
    {
        private static readonly IAlgoLogger _logger = AlgoLoggerFactory.GetLogger<ChildProcessRuntimeHost>();

        private const int AbortTimeout = 5000;

        private static readonly string _filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "AlgoRuntime", "TickTrader.Algo.RuntimeV1Host.NetFx.exe");

        private Process _process;
        private TaskCompletionSource<bool> _stopTaskSrc;
        private string _proxyId;


        public Task Start(string address, int port, string proxyId)
        {
            var startInfo = new ProcessStartInfo(_filePath)
            {
                UseShellExecute = false,
                CreateNoWindow = true,
                Arguments = string.Join(" ", address, port.ToString(), $"\"{proxyId}\""),
                WorkingDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory),
            };
            _process = Process.Start(startInfo);
            _process.EnableRaisingEvents = true;
            _process.Exited += OnExited;

            _proxyId = proxyId;
            _logger.Info($"{proxyId} host running in process {_process.Id}");

            return Task.CompletedTask;
        }

        public async Task Stop()
        {
            _stopTaskSrc = new TaskCompletionSource<bool>();
            StopInternal();
            var hasStopped = await _stopTaskSrc.Task.ConfigureAwait(false);
            _process.Exited -= OnExited;
            if (!hasStopped)
            {
                _logger.Error($"{_proxyId} host didn't stop within timeout. Killing process {_process.Id}...");
                _process.Kill();
            }
            _logger.Info($"{_proxyId} host stopped");
        }


        private void OnExited(object sender, EventArgs args)
        {
            _logger.Info($"{_proxyId} host exited with code {_process.ExitCode}");
            _stopTaskSrc?.TrySetResult(true);
        }

        private async void StopInternal()
        {
            if (_process.HasExited)
            {
                _stopTaskSrc.TrySetResult(true);
                return;
            }
            _logger.Info($"{_proxyId} wating for stop...");
            await Task.Delay(AbortTimeout).ConfigureAwait(false);
            _stopTaskSrc.TrySetResult(_process.HasExited);
        }
    }
}
