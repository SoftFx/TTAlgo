using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TickTrader.Algo.Common.Lib
{
    public static class TaskExt
    {
        public static Task WhenCanceled(this CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<bool>();
            cancellationToken.Register(s => ((TaskCompletionSource<bool>)s).SetCanceled(), tcs);
            return tcs.Task;
        }

        public static Task AddCancelation(this Task awaitable, CancellationToken cancellationToken)
        {
            return Task.WhenAny(awaitable, cancellationToken.WhenCanceled());
        }
    }
}
