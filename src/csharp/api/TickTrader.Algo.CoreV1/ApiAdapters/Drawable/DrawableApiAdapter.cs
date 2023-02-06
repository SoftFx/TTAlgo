using System;
using TickTrader.Algo.Api;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.CoreV1
{
    internal interface IDrawableUpdateSink
    {
        void Send(DrawableObjectUpdate upd);
    }


    internal class DrawableApiAdapter : IDrawableApi, IDrawableUpdateSink
    {
        private readonly DrawableCollection _collection;


        public IDrawableCollection LocalCollection => _collection;

        public Action<DrawableObjectUpdate> Updated { get; set; }


        public DrawableApiAdapter()
        {
            _collection = new DrawableCollection(this);
        }


        void IDrawableUpdateSink.Send(DrawableObjectUpdate upd) => Updated?.Invoke(upd);


        internal void FlushAll()
        {
            _collection.FlushAll();
        }
    }
}
