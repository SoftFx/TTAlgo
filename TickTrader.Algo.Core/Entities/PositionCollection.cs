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
    public class PositionCollection : IEnumerable<PositionAccessor>
    {
        private PluginBuilder _builder;
        private PositionsFixture _fixture = new PositionsFixture();

        internal NetPositionList PositionListImpl { get { return _fixture; } }

        public PositionCollection(PluginBuilder builder)
        {
            _builder = builder;
        }

        public PositionAccessor UpdatePosition(PositionExecReport eReport)
        {
            PositionAccessor pos;

            if (eReport.ExecAction == OrderExecAction.Closed)
            {
                pos = _fixture.RemovePosition(eReport.PositionInfo.Symbol);
                PositionRemoved?.Invoke(pos);
            }
            else
            {
                pos = _fixture.UpdatePosition(eReport.PositionInfo, _builder.Symbols.GetOrDefault);
                PositionUpdated?.Invoke(pos);
            }

            return pos;
        }

        public PositionAccessor GetPositionOrNull(string symbol)
        {
            return _fixture.GetOrDefault(symbol);
        }

        public event Action<PositionAccessor> PositionUpdated;
        public event Action<PositionAccessor> PositionRemoved;

        public void FirePositionUpdated(NetPositionModifiedEventArgs args)
        {
            _builder.InvokePluginMethod(() => _fixture.FirePositionModified(args));
        }

        public IEnumerator<PositionAccessor> GetEnumerator()
        {
            return _fixture.Values.GetEnumerator();
        }

        public void Clear()
        {
            _fixture.Clear();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _fixture.Values.GetEnumerator();
        }

        internal class PositionsFixture : NetPositionList
        {
            private ConcurrentDictionary<string, PositionAccessor> _positions = new ConcurrentDictionary<string, PositionAccessor>();

            public NetPosition this[string symbol]
            {
                get
                {
                    PositionAccessor entity;
                    if (!_positions.TryGetValue(symbol, out entity))
                        return PositionAccessor.CreateEmpty(symbol);
                    return entity;
                }
            }

            public int Count => _positions.Count;

            internal IEnumerable<PositionAccessor> Values => _positions.Values;

            public event Action<NetPositionModifiedEventArgs> Modified;

            public void FirePositionModified(NetPositionModifiedEventArgs args)
            {
                Modified?.Invoke(args);
            }

            public PositionAccessor UpdatePosition(PositionEntity entity, Func<string, Symbol> symbolProvider)
            {
                PositionAccessor pos;

                if (!_positions.TryGetValue(entity.Symbol, out pos))
                {
                    pos = new PositionAccessor(entity, symbolProvider);
                    _positions[entity.Symbol] = pos;
                }
                else
                    pos.Update(entity);

                return pos;
            }

            public PositionAccessor RemovePosition(string symbol)
            {
                PositionAccessor pos;
                _positions.TryRemove(symbol, out pos);
                return pos;
            }

            public PositionAccessor GetOrDefault(string symbol)
            {
                PositionAccessor entity;
                _positions.TryGetValue(symbol, out entity);
                return entity;
            }

            public IEnumerator<NetPosition> GetEnumerator()
            {
                return _positions.Values.GetEnumerator();
            }

            public void Clear()
            {
                _positions.Clear();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return _positions.Values.GetEnumerator();
            }
        }
    }
}
