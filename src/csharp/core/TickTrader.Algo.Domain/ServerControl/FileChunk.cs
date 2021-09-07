using Google.Protobuf;

namespace TickTrader.Algo.Domain.ServerControl
{
    public partial class FileChunk
    {
        public static readonly FileChunk FinalChunk = new FileChunk(-1, ByteString.Empty, true);


        public FileChunk(int id)
            : this(id, ByteString.Empty, false)
        {
        }

        public FileChunk(int id, ByteString binary)
            : this(id, binary, false)
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
