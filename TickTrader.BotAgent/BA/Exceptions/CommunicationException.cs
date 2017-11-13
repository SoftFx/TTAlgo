using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TickTrader.Algo.Common.Model;

namespace TickTrader.BotAgent.BA.Exceptions
{
    public class CommunicationException : BAException
    {
        public CommunicationException(string message, ConnectionErrorCodes fdkErrorCode) : base(message)
        {
            Code = ExceptionCodes.CommunicationError;
            FdkCode = fdkErrorCode;
        }

        public ConnectionErrorCodes FdkCode { get; }
    }
}
