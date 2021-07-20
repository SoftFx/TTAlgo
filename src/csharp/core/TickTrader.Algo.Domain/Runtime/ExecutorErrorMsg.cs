using System;

namespace TickTrader.Algo.Domain
{
    public partial class ExecutorErrorMsg
    {
        public ExecutorErrorMsg(string id, Exception ex)
        {
            Id = id;
            Message = ex.Message;
            Stacktrace = ex.StackTrace;
            ExceptionType = ex.GetType().FullName;
        }
    }
}
