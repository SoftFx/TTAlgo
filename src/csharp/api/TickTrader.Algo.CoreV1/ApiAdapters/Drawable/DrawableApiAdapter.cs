using System;
using TickTrader.Algo.Api;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.CoreV1
{
    internal interface IDrawableUpdateSink
    {
        // Calling code can check this flag to skip creating DrawableCollectionUpdate
        bool IsBatchBuild { get; }


        void Send(DrawableCollectionUpdate upd);
    }


    internal class DrawableApiAdapter : IDrawableApi, IDrawableUpdateSink
    {
        private readonly DrawableCollectionAdapter _collection;

        private bool _isBatch = false;


        public IDrawableCollection LocalCollection => _collection;

        public Action<DrawableCollectionUpdate> Updated { get; set; }


        public DrawableApiAdapter()
        {
            _collection = new DrawableCollectionAdapter(this);
        }


        bool IDrawableUpdateSink.IsBatchBuild => _isBatch;

        void IDrawableUpdateSink.Send(DrawableCollectionUpdate upd)
        {
            if (!_isBatch)
                Updated?.Invoke(upd);
        }

        internal void FlushAll() => _collection.FlushAll();

        internal void BeginBatch() => _isBatch = true;

        internal void EndBatch()
        {
            _isBatch = false;
            _collection.FlushAfterBatchBuild();
        }
    }
}
