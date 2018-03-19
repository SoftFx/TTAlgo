using System;
using System.Collections.Generic;
using System.Text;

namespace ActorSharp
{
    internal class LocalPage<T> : List<T>
    {
        public bool Last { get; set; }
        public Exception Error { get; set; }
    }

    internal class CloseWriterRequest
    {
        public CloseWriterRequest(Exception ex)
        {
            Error = ex;
        }

        public Exception Error { get; }
    }
}
