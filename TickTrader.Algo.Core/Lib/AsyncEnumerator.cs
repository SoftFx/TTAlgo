using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.Core.Lib
{
    public static class AsyncEnumerator
    {
        public static IAsyncEnumerator<T> Empty<T>()
        {
            return new EmptyAsyncEnumerator<T>();
        }

        public static IAsyncCrossDomainEnumerator<T> AsCrossDomain<T>(this IAsyncEnumerator<T[]> enumerator)
           where T : class
        {
            return new CrossDomainAdapter<T>(enumerator);
        }

        public static IAsyncEnumerator<T[]> AsPagedAsync<T>(this IAsyncCrossDomainEnumerator<T> enumerator)
        {
            if (enumerator == null)
                return Empty<T[]>();

            return new AsyncEnumeratorAdapter<T>(enumerator);
        }

        public static IAsyncEnumerator<TDst> Cast<TSrc, TDst>(this IAsyncEnumerator<TSrc> src)
            where TSrc : TDst
        {
            return new AsyncSelect<TSrc, TDst>(src, i => (TDst)i);
        }

        public static IAsyncEnumerator<TDst> Select<TSrc, TDst>(this IAsyncEnumerator<TSrc> src, Func<TSrc, TDst> selector)
        {
            return new AsyncSelect<TSrc, TDst>(src, selector);
        }

        public static IEnumerable<T> ToEnumerable<T>(this Func<IAsyncCrossDomainEnumerator<T>> factory)
        {
            var enumerator = factory();

            if (enumerator == null)
                yield break;

            while (true)
            {
                var callback = new CrossDomainTaskProxy<T[]>();
                enumerator.GetNextPage(callback);
                var page = callback.Result;

                foreach (var i in page)
                    yield return i;
            }
        }

        internal class CrossDomainAdapter<T> : IAsyncCrossDomainEnumerator<T>
            where T : class
        {
            private IAsyncEnumerator<T[]> _enumerator;

            public CrossDomainAdapter(IAsyncEnumerator<T[]> enumerator)
            {
                _enumerator = enumerator;
            }

            public void Dispose()
            {
                _enumerator.Dispose();
            }

            public void GetNextPage(CrossDomainTaskProxy<T[]> pageCallback)
            {
                _enumerator.Next().ContinueWith(t =>
                {
                    if (t.IsFaulted)
                        pageCallback.SetException(t.Exception);
                    else if (t.Result)
                        pageCallback.SetResult(_enumerator.Current);
                    else
                        pageCallback.SetResult(null);
                });
            }
        }

        internal class AsyncEnumeratorAdapter<T> : IAsyncEnumerator<T[]>
        {
            private IAsyncCrossDomainEnumerator<T> _proxy;

            public T[] Current { get; private set; }

            public AsyncEnumeratorAdapter(IAsyncCrossDomainEnumerator<T> proxy)
            {
                _proxy = proxy;
            }

            public async Task<T[]> GetNextPage()
            {
                using (var taskProxy = new CrossDomainTaskProxy<T[]>())
                {
                    _proxy.GetNextPage(taskProxy);
                    return await taskProxy.Task;
                }
            }

            public void Dispose()
            {
                _proxy.Dispose();
            }

            public async Task<bool> Next()
            {
                using (var taskProxy = new CrossDomainTaskProxy<T[]>())
                {
                    _proxy.GetNextPage(taskProxy);
                    Current = await taskProxy.Task;
                    return Current != null;
                }
            }
        }

        internal class AsyncSelect<TSrc, TDst> : IAsyncEnumerator<TDst>
        {
            private IAsyncEnumerator<TSrc> _srcEnumerator;
            private Func<TSrc, TDst> _selector;

            public AsyncSelect(IAsyncEnumerator<TSrc> srcEnumerator, Func<TSrc, TDst> selector)
            {
                _srcEnumerator = srcEnumerator;
                _selector = selector;
            }

            public TDst Current => _selector(_srcEnumerator.Current);

            public void Dispose()
            {
                _srcEnumerator.Dispose();
            }

            public Task<bool> Next()
            {
                return _srcEnumerator.Next();
            }
        }

        internal class EmptyAsyncEnumerator<T> : IAsyncEnumerator<T>
        {
            public T Current => default(T);

            public void Dispose()
            {
            }

            public Task<bool> Next()
            {
                return Task.FromResult(false);
            }
        }
    }
}
