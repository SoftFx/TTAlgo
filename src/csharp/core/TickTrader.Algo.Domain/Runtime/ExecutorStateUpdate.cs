namespace TickTrader.Algo.Domain
{
    public partial class ExecutorStateUpdate
    {
        public ExecutorStateUpdate(string executorId, Executor.Types.State oldState, Executor.Types.State newState)
        {
            ExecutorId = executorId;
            OldState = oldState;
            NewState = newState;
        }
    }
}
