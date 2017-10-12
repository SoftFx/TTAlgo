using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.Common.Lib
{
    public static class EnumerableExt
    {
        public static IEnumerable<T> GetSyncWrapper<T>(this IEnumerable<T> src, object lockObj)
        {
            IEnumerator<T> enumerator;
            T current;

            lock (lockObj)
            {
                enumerator = src.GetEnumerator();
                if (!enumerator.MoveNext())
                    yield break;
                current = enumerator.Current;
            }

            while (true)
            {
                yield return current;

                lock (lockObj)
                {
                    if (!enumerator.MoveNext())
                        break;

                    current = enumerator.Current;
                }
            }
        }

        public static IAsyncEnumerator<T> ToAsyncEnumerator<T>(this IEnumerable<Task<T>> srcEnumerable)
        {
            return new AsyncAdapter<T>(srcEnumerable);
        }

        private class AsyncAdapter<T> : IAsyncEnumerator<T>, IDisposable
        {
            private IEnumerator<Task<T>> _srcEnumerator;

            public AsyncAdapter(IEnumerable<Task<T>> enumerable)
            {
                _srcEnumerator = enumerable.GetEnumerator();
            }

            public T Current { get; private set; }

            public void Dispose()
            {
                _srcEnumerator.Dispose();
            }

            public Task<bool> Next()
            {
                if (!_srcEnumerator.MoveNext())
                    return Task.FromResult(false);
                else
                    return _srcEnumerator.Current.ContinueWith(t => { Current = t.Result; return true; });
            }
        }
    }
}
