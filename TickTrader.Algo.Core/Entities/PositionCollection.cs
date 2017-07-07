using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.Core
{
    public class PositionCollection : IEnumerable<PositionEntity>
    {
        private PluginBuilder _builder;
        private PositionsFixture _fixture = new PositionsFixture();

        internal NetPositionList PositionListImpl { get { return _fixture; } }

        public PositionCollection(PluginBuilder builder)
        {
            _builder = builder;
        }

        public PositionEntity UpdatePosition(PositionExecReport eReport)
        {
            PositionEntity pos;

            if (eReport.ExecAction == OrderExecAction.Closed)
            {
                pos = _fixture.RemovePosition(eReport.Symbol);
                PositionRemoved?.Invoke(pos);
            }
            else
            {
                pos = _fixture.UpdatePosition(eReport);
                PositionUpdated?.Invoke(pos);
            }

            return pos;
        }

        public PositionEntity GetPositionOrNull(string symbol)
        {
            return _fixture.GetOrDefault(symbol);
        }

        public event Action<PositionEntity> PositionUpdated;
        public event Action<PositionEntity> PositionRemoved;

        public void FirePositionUpdated(NetPositionModifiedEventArgs args)
        {
            _builder.InvokePluginMethod(() => _fixture.FirePositionModified(args));
        }

        public IEnumerator<PositionEntity> GetEnumerator()
        {
            return _fixture.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _fixture.Values.GetEnumerator();
        }

        internal class PositionsFixture : NetPositionList
        {
            private ConcurrentDictionary<string, PositionEntity> _positions = new ConcurrentDictionary<string, PositionEntity>();

            public NetPosition this[string symbol]
            {
                get
                {
                    PositionEntity entity;
                    if (!_positions.TryGetValue(symbol, out entity))
                        return PositionEntity.CreateEmpty(symbol);
                    return entity;
                }
            }

            public int Count => _positions.Count;

            internal IEnumerable<PositionEntity> Values => _positions.Values;

            public event Action<NetPositionModifiedEventArgs> Modified;

            public void FirePositionModified(NetPositionModifiedEventArgs args)
            {
                Modified?.Invoke(args);
            }

            public PositionEntity UpdatePosition(PositionExecReport eReport)
            {
                PositionEntity pos;

                if (!_positions.TryGetValue(eReport.Symbol, out pos))
                {
                    pos = new PositionEntity(eReport);
                    _positions[eReport.Symbol] = pos;
                }
                else
                    pos.Update(eReport);

                return pos;
            }

            public PositionEntity RemovePosition(string symbol)
            {
                PositionEntity pos;
                _positions.TryRemove(symbol, out pos);
                return pos;
            }

            public PositionEntity GetOrDefault(string symbol)
            {
                PositionEntity entity;
                _positions.TryGetValue(symbol, out entity);
                return entity;
            }

            public IEnumerator<NetPosition> GetEnumerator()
            {
                return _positions.Values.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return _positions.Values.GetEnumerator();
            }
        }
    }
}
