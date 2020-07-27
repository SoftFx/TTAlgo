using System;

namespace TickTrader.Algo.Domain
{
    public partial class UnitError
    {
        public UnitError(Exception ex)
        {
            Message = ex.Message;
            Stacktrace = ex.StackTrace;
            ExceptionType = ex.GetType().FullName;
        }
    }
}
