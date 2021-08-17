using Google.Protobuf;

namespace TickTrader.Algo.Server.PublicAPI
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


    public partial class FileChunk
    {
        public static readonly FileChunk FinalChunk = new FileChunk(-1, ByteString.Empty, true);


        public FileChunk(int id)
            : this(id, ByteString.Empty, false)
        {
        }

        public FileChunk(int id, ByteString binary, bool isFinal)
        {
            Id = id;
            Binary = binary;
            IsFinal = isFinal;
        }
    }
}
