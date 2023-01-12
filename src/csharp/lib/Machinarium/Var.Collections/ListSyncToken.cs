using System.Collections.Generic;
using System.Threading;

namespace Machinarium.Qnil
{
    internal class ListSyncToken<T> : IVarList<T>
    {
        private readonly IVarList<T> _src;
        private readonly SynchronizationContext _syncContext;

        public IReadOnlyList<T> Snapshot => _src.Snapshot;

        public event ListUpdateHandler<T> Updated;


        public ListSyncToken(IVarList<T> src, SynchronizationContext syncContext)
        {
            _src = src;
            _syncContext = syncContext;

            _src.Updated += Src_Updated;
        }


        public void Dispose()
        {
            _src.Updated -= Src_Updated;
            _src.Dispose();
        }


        private void Src_Updated(ListUpdateArgs<T> args)
        {
            _syncContext.Post(_ => Updated?.Invoke(args), null);
        }
    }
}
