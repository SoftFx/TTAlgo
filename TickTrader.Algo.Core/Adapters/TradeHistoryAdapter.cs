using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;
using TickTrader.Algo.Core.Lib;

namespace TickTrader.Algo.Core
{
    internal class TradeHistoryAdapter : TradeHistory
    {
        private ITradeHistoryProvider _provider;

        public ITradeHistoryProvider Provider { get { return _provider; } set { _provider = value; } }

        public IEnumerator<TradeReport> GetEnumerator()
        {
            if (_provider != null)
                return new EnumeratorAdapter<TradeReport>(_provider.GetTradeHistory(false));
            else
                return Enumerable.Empty<TradeReport>().GetEnumerator();
        }

        public IEnumerable<TradeReport> Get(bool skipCancelOrders = false)
        {
            if (_provider != null)
                return new EnumerableAdapter<TradeReport>(() => _provider.GetTradeHistory(skipCancelOrders));
            else
                return Enumerable.Empty<TradeReport>();
        }

        public IEnumerable<TradeReport> GetRange(DateTime from, DateTime to, bool skipCancelOrders)
        {
            if (_provider != null)
                return new EnumerableAdapter<TradeReport>(() => _provider.GetTradeHistory(from, to, skipCancelOrders));
            else
                return Enumerable.Empty<TradeReport>();
        }

        public IEnumerable<TradeReport> GetRange(DateTime to, bool skipCancelOrders)
        {
            if (_provider != null)
                return new EnumerableAdapter<TradeReport>(() => _provider.GetTradeHistory(to, skipCancelOrders));
            else
                return Enumerable.Empty<TradeReport>();
        }

        public IAsyncEnumerator<TradeReport[]> GetAsync(bool skipCancelOrders)
        {
            if (_provider != null)
                return new AsyncEnumeratorAdapter<TradeReport>(_provider.GetTradeHistory(skipCancelOrders));
            else
                return new EmptyAsyncEnumerator<TradeReport[]>();
        }

        public IAsyncEnumerator<TradeReport[]> GetRangeAsync(DateTime from, DateTime to, bool skipCancelOrders)
        {
            if (_provider != null)
                return new AsyncEnumeratorAdapter<TradeReport>(_provider.GetTradeHistory(from, to, skipCancelOrders));
            else
                return new EmptyAsyncEnumerator<TradeReport[]>();
        }

        public IAsyncEnumerator<TradeReport[]> GetRangeAsync(DateTime to, bool skipCancelOrders)
        {
            if (_provider != null)
                return new AsyncEnumeratorAdapter<TradeReport>(_provider.GetTradeHistory(to, skipCancelOrders));
            else
                return new EmptyAsyncEnumerator<TradeReport[]>();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    internal class EnumerableAdapter<T> : IEnumerable<T> where T : class
    {
        private Func<IAsyncCrossDomainEnumerator<T>> _enumerableFactory;

        public EnumerableAdapter(Func<IAsyncCrossDomainEnumerator<T>> enumerableFactory)
        {
            _enumerableFactory = enumerableFactory;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return new EnumeratorAdapter<T>(_enumerableFactory());
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    internal class EnumeratorAdapter<T> : IEnumerator<T> where T : class
    {
        private IAsyncCrossDomainEnumerator<T> _asyncEnumerator;
        private bool isStarted;
        private T[] _currentPage;
        private int _currentPageIndex;

        public EnumeratorAdapter(IAsyncCrossDomainEnumerator<T> asyncEnumerator)
        {
            _asyncEnumerator = asyncEnumerator;
        }

        public T Current => _currentPage[_currentPageIndex];
        object IEnumerator.Current => Current;

        public void Dispose()
        {
            _asyncEnumerator.Dispose();
        }

        public bool MoveNext()
        {
            if (_currentPage == null)
            {
                if (isStarted)
                    return false;
                else
                {
                    isStarted = true;
                    return LoadNextPage();
                }
            }
            else if (_currentPageIndex < _currentPage.Length - 1)
            {
                _currentPageIndex++;
                return true;
            }
            else
                return LoadNextPage();
        }

        private bool LoadNextPage()
        {
            _currentPage = null;
            _currentPageIndex = 0;
            using (var task = new CrossDomainTaskProxy<T[]>())
            {
                _asyncEnumerator.GetNextPage(task);
                _currentPage = task.Result;
            }
            if (_currentPage != null &&_currentPage.Length == 0)
                _currentPage = null;
            return _currentPage != null;
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }
    }

    internal class AsyncEnumeratorAdapter<T> : IAsyncEnumerator<T[]> where  T: class
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

    internal class EmptyAsyncEnumerator<T> : IAsyncEnumerator<T> where T : class
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
