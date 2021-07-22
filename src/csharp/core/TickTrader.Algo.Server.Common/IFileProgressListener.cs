namespace TickTrader.Algo.Server.Common
{
    public interface IFileProgressListener
    {
        void Init(long initialProgress);

        void IncrementProgress(long progressValue);
    }
}
