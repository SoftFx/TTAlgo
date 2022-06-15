using System;
using System.Threading;

namespace TickTrader.Algo.Core.Lib
{
    public class SubList<T>
        where  T : class
    {
        private readonly object _lock = new object();

        private T[] _subs = new T[0];


        public ReadOnlySpan<T> Items => _subs;


        public void AddSub(T sub)
        {
            lock (_lock)
            {
                var subs = _subs;
                var cnt = subs.Length;

                var newSubs = new T[cnt + 1];
                for (var i = 0; i < cnt; i++) newSubs[i] = subs[i];
                newSubs[cnt] = sub;

                Volatile.Write(ref _subs, newSubs);
            }
        }

        public void RemoveSub(T sub)
        {
            lock (_lock)
            {
                var subs = _subs;
                var cnt = subs.Length;

                var newSubs = new T[cnt - 1];
                var j = 0;
                for (var i = 0; i < cnt; i++)
                {
                    var item = subs[i];
                    if (!ReferenceEquals(item, sub))
                    {
                        newSubs[j] = item;
                        j++;
                    }
                }

                Volatile.Write(ref _subs, newSubs);
            }
        }
    }
}
