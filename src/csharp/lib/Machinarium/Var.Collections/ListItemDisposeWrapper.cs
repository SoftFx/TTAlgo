using System;
using System.Collections.Generic;

namespace Machinarium.Qnil
{
    internal class ListItemDisposeWrapper<TValue> : IVarList<TValue>
        where TValue : IDisposable
    {
        private readonly IVarList<TValue> _src;


        public IReadOnlyList<TValue> Snapshot => _src.Snapshot;

        public event ListUpdateHandler<TValue> Updated;


        public ListItemDisposeWrapper(IVarList<TValue> src)
        {
            _src = src;

            src.Updated += Src_Updated;
        }


        public void Dispose()
        {
            _src.Updated -= Src_Updated;
            foreach (var item in _src.Snapshot)
            {
                item.Dispose();
            }
            OnUpdated(new ListUpdateArgs<TValue>(this, DLinqAction.Dispose));
            _src.Dispose();
        }


        private void OnUpdated(ListUpdateArgs<TValue> args) => Updated?.Invoke(args);

        private void Src_Updated(ListUpdateArgs<TValue> args)
        {
            if (args.Action == DLinqAction.Dispose)
                Dispose();
            else
            {
                try
                {
                    OnUpdated(args);
                }
                finally
                {
                    if (args.Action == DLinqAction.Remove)
                    {
                        args.OldItem.Dispose();
                    }
                    else if (args.Action == DLinqAction.Replace
                        && !ReferenceEquals(args.OldItem, args.NewItem))
                    {
                        args.OldItem.Dispose();
                    }
                }
            }
        }
    }
}
