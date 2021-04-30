using System;
using System.Threading.Tasks;

namespace TickTrader.Algo.Isolation.NetFx
{
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
