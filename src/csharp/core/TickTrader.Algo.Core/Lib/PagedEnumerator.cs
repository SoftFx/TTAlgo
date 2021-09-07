using System;
using System.Collections.Generic;

namespace TickTrader.Algo.Core.Lib
{
    public interface IPagedEnumerator<T>
    {
        List<T> GetNextPage();
    }

    public static class PagedEnumerator
    {
        public static IPagedEnumerator<T> GetPagedEnumerator<T>(this IEnumerable<T> src, int pageSize)
        {
            return new Adapter<T>(src.GetEnumerator(), pageSize);
        }

        public static IEnumerable<T> JoinPages<T>(this IPagedEnumerator<T> pagedEnumerator)
        {
            while (true)
            {
                var page = pagedEnumerator.GetNextPage();
                if (page.Count == 0)
                    yield break;

                foreach (var e in page)
                    yield return e;
            }
        }

        public static IEnumerable<T> JoinPages<T>(this IPagedEnumerator<T> pagedEnumerator, Action<int> pageReadCallback)
        {
            var totalCount = 0;

            while (true)
            {
                var page = pagedEnumerator.GetNextPage();
                if (page.Count == 0)
                    yield break;

                foreach (var e in page)
                    yield return e;

                totalCount += page.Count;
                pageReadCallback(totalCount);
            }
        }

        private class Adapter<T> : IPagedEnumerator<T>
        {
            private IEnumerator<T> _src;
            private int _pageSize;

            public Adapter(IEnumerator<T> src, int pageSize)
            {
                _src = src;
                _pageSize = pageSize;
            }

            public List<T> GetNextPage()
            {
                var page = new List<T>(_pageSize);

                for (int i = 0; i < _pageSize; i++)
                {
                    if (!_src.MoveNext())
                        break;

                    page.Add(_src.Current);
                }

                return page;
            }
        }
    }
}
