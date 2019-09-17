using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Core.Lib
{
    public class CrossDomainTaskProxy<T> : CrossDomainObject
    {
        private TaskCompletionSource<T> _taskSrc = new TaskCompletionSource<T>();

        public Task<T> Task => _taskSrc.Task;
        public T Result => _taskSrc.Task.Result;

        public void SetResult(T result)
        {
            _taskSrc.SetResult(result);
        }

        public bool TrySetResult(T result)
        {
            return _taskSrc.TrySetResult(result);
        }

        public void SetException(Exception ex)
        {
            _taskSrc.SetException(ex);
        }

    }

    public class CrossDomainTaskProxy : CrossDomainObject, ICallback
    {
        private TaskCompletionSource<object> _taskSrc = new TaskCompletionSource<object>();

        public Task Task => _taskSrc.Task;

        public void SetCompleted()
        {
            _taskSrc.SetResult(null);
            Dispose();
        }

        public bool TrySetCompleted()
        {
            return _taskSrc.TrySetResult(null);
        }

        public void SetException(Exception ex)
        {
            _taskSrc.SetException(ex);
        }

        public void Invoke()
        {
            SetCompleted();
        }
    }
}
