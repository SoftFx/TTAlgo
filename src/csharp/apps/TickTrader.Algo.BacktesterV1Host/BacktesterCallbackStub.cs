using System;
using System.Threading.Tasks;
using TickTrader.Algo.BacktesterApi;

namespace TickTrader.Algo.BacktesterV1Host
{
    internal class BacktesterCallbackStub : IBacktesterV1Callback
    {
        private TaskCompletionSource<object> _awaitStopSrc = new TaskCompletionSource<object>();


        public Task AwaitStop() => _awaitStopSrc.Task;


        public void SendProgress(double current, double total)
        {
            Console.WriteLine($"Progress: {current}/{total}");
        }

        public void SendStoppedMsg(string message)
        {
            Console.WriteLine($"Stopped: {message}");
            _awaitStopSrc.TrySetResult(message);
        }

        public void SendStateUpdate(EmulatorStates state) { }
    }
}
