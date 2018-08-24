using System;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using System.Text;

namespace ActorSharp
{
    internal class LocalPage<T> : List<T>
    {
        public bool Last { get; set; }
        public ExceptionDispatchInfo Error { get; set; }
    }

    internal class CloseWriterRequest
    {
        public CloseWriterRequest(Exception ex)
        {
            if (ex != null)
                Error = ExceptionDispatchInfo.Capture(ex);
        }

        public ExceptionDispatchInfo Error { get; }
    }
}
