using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Machinarium.ActorModel
{
    public class LameActorCore : ActorCore
    {
        public LameActorCore()
        {
        }

        public LameActorCore(ActorContextOptions contextOption)
            : base(contextOption)
        {
        }

        public Task<TResult> AsyncCall<TResult>(Func<Task<TResult>> asyncFunction)
        {
            TaskCompletionSource<TResult> src = new TaskCompletionSource<TResult>();
            Enqueue(() => asyncFunction().ContinueWith(t =>
            {
                if (t.IsFaulted)
                    src.SetException(t.Exception);
                else if (t.IsCanceled)
                    src.SetCanceled();
                else
                    src.SetResult(t.Result);
            }));
            return src.Task;
        }

        public Task AsyncCall(Func<Task> asyncFunction)
        {
            TaskCompletionSource<object> src = new TaskCompletionSource<object>();
            Enqueue(() => asyncFunction().ContinueWith(t =>
            {
                if (t.IsFaulted)
                    src.SetException(t.Exception);
                else if (t.IsCanceled)
                    src.SetCanceled();
                else
                    src.SetResult(this);
            }));
            return src.Task;
        }
    }
}
