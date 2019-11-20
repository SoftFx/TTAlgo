using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.Core.Lib
{
    public static class AsyncEnumerator
    {
        private static readonly Task<bool> CompletedTrue = Task.FromResult(true);
        private static readonly Task<bool> CompletedFalse = Task.FromResult(false);

        public static IAsyncEnumerator<T> Empty<T>()
        {
            return new EmptyAsyncEnumerator<T>();
        }

        public static IAsyncCrossDomainEnumerator<T> AsCrossDomain<T>(this IAsyncEnumerator<T[]> enumerator)
           where T : class
        {
            return new CrossDomainAdapter<T>(enumerator);
        }

        public static IAsyncEnumerator<T> AsAsync<T>(this IAsyncCrossDomainEnumerator<T> enumerator)
        {
            if (enumerator == null)
                return Empty<T>();

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

        public static AsyncEnumerableChannelAdapter<T> GetAdapter<T>(Func<IAsyncCrossDomainEnumerator<T>> factory) where T : class => new AsyncEnumerableChannelAdapter<T>(factory);

        public static IAsyncEnumerator<T> SimulateAsync<T>(this IEnumerable<T> src)
        {
            return new FakeAsyncEnumerator<T>(src);
        }

        internal class CrossDomainAdapter<T> : CrossDomainObject, IAsyncCrossDomainEnumerator<T>
            where T : class
        {
            private IAsyncEnumerator<T[]> _enumerator;

            public CrossDomainAdapter(IAsyncEnumerator<T[]> enumerator)
            {
                _enumerator = enumerator;
            }

            public override void Dispose()
            {
                _enumerator.Dispose();

                base.Dispose();
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

        internal class AsyncEnumeratorAdapter<T> : IAsyncEnumerator<T>
        {
            private static readonly Task<bool> TrueResult = Task.FromResult(true);
            private static readonly Task<bool> FalseResult = Task.FromResult(false);

            private IAsyncCrossDomainEnumerator<T> _proxy;
            private T[] _page;
            private int _index;

            public T Current { get; private set; }

            public AsyncEnumeratorAdapter(IAsyncCrossDomainEnumerator<T> proxy)
            {
                _proxy = proxy;
            }

            private async Task<bool> GetNextPage()
            {
                using (var taskProxy = new CrossDomainTaskProxy<T[]>())
                {
                    _proxy.GetNextPage(taskProxy);
                    _page = await taskProxy.Task;
                    if (_page != null && _page.Length > 0)
                    {
                        _index = 1;
                        Current = _page[0];
                        return true;
                    }
                    else
                    {
                        _index = -1;
                        return false;
                    }
                }
            }

            public void Dispose()
            {
                _proxy.Dispose();
            }

            public Task<bool> Next()
            {
                if (_index == -1)
                    return FalseResult;

                if (_page == null || _index >= _page.Length)
                    return GetNextPage();

                Current = _page[_index++];
                return TrueResult;
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

        internal class FakeAsyncEnumerator<T> : IAsyncEnumerator<T>
        {
            private IEnumerator<T> _e;

            public FakeAsyncEnumerator(IEnumerable<T> src)
            {
                _e = src.GetEnumerator();
            }

            public T Current => _e.Current;

            public void Dispose()
            {
                _e.Dispose();
            }

            public Task<bool> Next()
            {
                if (_e.MoveNext())
                    return CompletedTrue;
                return CompletedFalse;
            }
        }

        public class AsyncEnumerableChannelAdapter<T> : IEnumerable<T> where T : class
        {
            private readonly Func<IAsyncCrossDomainEnumerator<T>> _factory;

            public AsyncEnumerableChannelAdapter(Func<IAsyncCrossDomainEnumerator<T>> factory)
            {
                _factory = factory;
            }

            public IEnumerator<T> GetEnumerator() => new AsyncEnumeratorChannelAdapter<T>(_factory);

            IEnumerator IEnumerable.GetEnumerator() => new AsyncEnumeratorChannelAdapter<T>(_factory);
        }

        public class AsyncEnumeratorChannelAdapter<T> : IEnumerator<T>, IDisposable where T : class
        {
            private IAsyncCrossDomainEnumerator<T> _enumerator;

            private List<T> _page;
            private int _position = -1;

            public T Current => _page?[_position];

            object IEnumerator.Current => _page?[_position];

            public AsyncEnumeratorChannelAdapter(Func<IAsyncCrossDomainEnumerator<T>> factory)
            {
                _enumerator = factory();
            }

            public void Dispose()
            {
                _enumerator.Dispose();
            }

            public bool MoveNext()
            {
                if (_enumerator == null)
                    return false;

                if (_page == null || _position == _page.Count - 1)
                {
                    var callback = new CrossDomainTaskProxy<T[]>();
                    _enumerator.GetNextPage(callback);
                    _page = callback.Result?.ToList();
                    _position = -1;

                    if (_page == null)
                        return false;
                }

                return ++_position < _page.Count;
            }

            public void Reset()
            {
                _page = null;
                _position = -1;
            }
        }
    }
}
