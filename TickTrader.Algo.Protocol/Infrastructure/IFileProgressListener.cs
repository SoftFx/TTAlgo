namespace TickTrader.Algo.Protocol
{
    public interface IFileProgressListener
    {
        void Init(long initialProgress);

        void IncrementProgress(long progressValue);
    }
}
