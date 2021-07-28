namespace TickTrader.Algo.Server.PublicAPI
{
    public interface IFileProgressListener
    {
        void Init(long initialProgress);

        void IncrementProgress(long progressValue);
    }
}
