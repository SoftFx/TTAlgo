namespace TickTrader.Algo.Domain.ServerControl
{
    public partial class FileTransferSettings
    {
        public const int DefaultSize = 512 * 1024;

        public static FileTransferSettings Default => new FileTransferSettings(DefaultSize, 0);


        public FileTransferSettings(int chunkSize, int chunkOffset)
        {
            ChunkSize = chunkSize;
            ChunkOffset = chunkOffset;
        }
    }
}
