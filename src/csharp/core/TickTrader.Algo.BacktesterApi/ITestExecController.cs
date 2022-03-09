using System;

namespace TickTrader.Algo.BacktesterApi
{
    public enum EmulatorStates { WarmingUp, Running, Paused, Stopping, Stopped }

    public interface ITestExecController
    {
        EmulatorStates State { get; }
        event Action<EmulatorStates> StateChanged;
        event Action<Exception> ErrorOccurred;
        void Pause();
        void Resume();
        void SetExecDelay(int delayMs);
    }
}
