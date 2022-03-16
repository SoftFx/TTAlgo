using System.Threading.Tasks;
using TickTrader.Algo.BacktesterApi;

namespace TickTrader.Algo.BacktesterV1Host
{
    internal class BacktesterCallbackStub : IBacktesterV1Callback
    {
        private TaskCompletionSource<object> _awaitStopSrc = new TaskCompletionSource<object>();


        public Task AwaitStop() => _awaitStopSrc.Task;


        public void SendProgress(double current, double total) { }

        public void SendStoppedMsg(string message)
        {
            _awaitStopSrc.TrySetResult(message);
        }

        public void SendStateUpdate(Emulator.Types.State state) { }
    }
}
