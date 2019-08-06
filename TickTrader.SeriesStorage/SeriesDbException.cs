using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.SeriesStorage
{
    public enum SeriesDbErrorCodes
    {
        None                = 0,
        Unknown             = 1,
        DatabaseMissing     = 101,
        DatabaseCorrupted   = 102,
        
    }

    public class SeriesDbException : Exception
    {
        public SeriesDbException(string message, SeriesDbErrorCodes errorCode, Exception innerException = null)
            : base(message, innerException)
        {
            ErrorCode = errorCode;
        }

        public SeriesDbErrorCodes ErrorCode { get; }
    }

    public class DbMissingException : SeriesDbException
    {
        public DbMissingException(string message)
            : base(message, SeriesDbErrorCodes.DatabaseMissing)
        {
        }
    }
}
