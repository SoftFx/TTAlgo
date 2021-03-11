using System;

namespace TickTrader.Algo.Domain
{
    public partial class PluginError
    {
        public PluginError(Exception ex)
        {
            Message = ex.Message;
            Stacktrace = ex.StackTrace;
            ExceptionType = ex.GetType().FullName;
        }
    }
}
