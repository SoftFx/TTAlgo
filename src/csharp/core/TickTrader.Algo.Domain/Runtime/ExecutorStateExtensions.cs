namespace TickTrader.Algo.Domain
{
    public static class ExecutorStateExtensions
    {
        public static bool IsStopped(this Executor.Types.State state) => state == Executor.Types.State.Stopped;

        public static bool IsWaitConnect(this Executor.Types.State state) => state == Executor.Types.State.WaitConnect;

        public static bool IsRunning(this Executor.Types.State state) => state == Executor.Types.State.Running;

        public static bool IsWaitReconnect(this Executor.Types.State state) => state == Executor.Types.State.WaitReconnect;

        public static bool IsStopping(this Executor.Types.State state) => state == Executor.Types.State.Stopping;

        public static bool IsFaulted(this Executor.Types.State state) => state == Executor.Types.State.Faulted;
    }
}
