namespace TickTrader.Algo.ServerControl
{
    public interface IFileProgressListener
    {
        void Init(long initialProgress);

        void IncrementProgress(long progressValue);
    }
}
