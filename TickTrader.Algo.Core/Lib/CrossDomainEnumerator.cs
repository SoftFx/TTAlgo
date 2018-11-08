using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Core.Lib
{
    public interface IPagedEnumerator<T> : IDisposable
    {
        List<T> GetNextPage();
    }

    public static class CrossDomainEnumerator
    {
        public static IPagedEnumerator<T> GetCrossDomainEnumerator<T>(this IEnumerable<T> src, int pageSize)
        {
            return new Adapter<T>(src.GetEnumerator(), pageSize);
        }

        public static IPagedEnumerator<T> AsCrossDomain<T>(this IEnumerator<T> src, int pageSize)
        {
            return new Adapter<T>(src, pageSize);
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

        private class Adapter<T> : CrossDomainObject, IPagedEnumerator<T>
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
