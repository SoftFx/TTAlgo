using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Core
{
    [Serializable]
    public class AlgoException : Exception
    {
        public AlgoException(string message) : base(message)
        {
        }

        protected AlgoException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }

    [Serializable]
    public class ExecutorException : AlgoException
    {
        public ExecutorException(string msg) : base(msg)
        {
        }

        protected ExecutorException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }

    [Serializable]
    public class AlgoOperationCanceledException : AlgoException
    {
        public AlgoOperationCanceledException(string msg) : base(msg)
        {
        }

        protected AlgoOperationCanceledException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }

    [Serializable]
    public class NotEnoughDataException : AlgoException
    {
        public NotEnoughDataException(string msg) : base(msg)
        {
        }

        protected NotEnoughDataException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
