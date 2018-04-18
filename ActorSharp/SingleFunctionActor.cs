using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ActorSharp
{
    internal class SingleFunctionActor : TaskCompletionSource<object>
    {
        private Func<Task> _asyncFunc;

        public SingleFunctionActor(Func<Task> asyncFunc)
        {
            _asyncFunc = asyncFunc;
        }

        public void Start()
        {
            try
            {
                _asyncFunc()
                    .ContinueWith(t =>
                    {
                        if (t.IsFaulted)
                            SetException(t.Exception);
                        else if (t.IsCanceled)
                            SetCanceled();
                        else
                            SetResult(null);
                    });
            }
            catch (Exception ex)
            {
                SetException(ex);
            }
        }
    }
}
