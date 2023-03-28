using System.Collections.Generic;
using System.Linq;
using TickTrader.Algo.CoreV1;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Core.Tests.Drawables
{
    public class DrawableTestContext
    {
        private readonly UpdateWrapper _updateSrc;
        private readonly DrawableCollectionAdapter _adapter;


        public Api.IDrawableCollection Collection => _adapter;

        public IReadOnlyList<DrawableCollectionUpdate> Updates => _updateSrc.Updates;


        private DrawableTestContext()
        {
            _updateSrc = new UpdateWrapper();
            _adapter = new DrawableCollectionAdapter(_updateSrc);
        }


        public static (DrawableTestContext, Api.IDrawableCollection) Create()
        {
            var ctx = new DrawableTestContext();
            return (ctx, ctx.Collection);
        }

        public static string[] GetObjectNames(int cnt, string pattern) => Enumerable.Range(0, cnt).Select(i => pattern + i).ToArray();


        public void FlushUpdates() => _adapter.FlushAll();

        public void ResetUpdates() => _updateSrc.Reset();

        public void FlushAndResetUpdates()
        {
            _adapter.FlushAll();
            _updateSrc.Reset();
        }


        private class UpdateWrapper : IDrawableUpdateSink
        {
            public List<DrawableCollectionUpdate> Updates { get; set; } = new List<DrawableCollectionUpdate>();


            public void Send(DrawableCollectionUpdate upd) => Updates.Add(upd);

            public void Reset() => Updates.Clear();
        }
    }
}
