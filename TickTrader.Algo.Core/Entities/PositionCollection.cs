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

        public PositionAccessor UpdatePosition(PositionEntity eReport)
        {
            PositionAccessor pos;

            if (eReport.Volume <= 0)
                pos = _fixture.RemovePosition(eReport.Symbol);
            else
            {
                pos = GetOrCreatePosition(eReport.Symbol);
                pos.Update(eReport);
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

        #region Emulation

        internal PositionAccessor GetOrCreatePosition(string symbol)
        {
            var smbInfo = _builder.Symbols.GetOrDefault(symbol);
            if (smbInfo == null)
                throw new OrderValidationError("Symbol Not Found:  " + symbol, OrderCmdResultCodes.SymbolNotFound);

            var pos = _fixture.GetOrDefault(symbol);

            if (pos == null)
            {
                pos = _fixture.CreatePosition(smbInfo);
                pos.Changed += Pos_Changed;
                pos.Removed += Pos_Removed;
            }

            return pos;
        }

        private void Pos_Removed(PositionAccessor pos)
        {
            pos.Changed -= Pos_Changed;
            pos.Removed -= Pos_Removed;
            var removed = _fixture.RemovePosition(pos.Symbol);
            if (removed != null)
                PositionRemoved?.Invoke(pos);
        }

        private void Pos_Changed(PositionAccessor pos)
        {
            PositionUpdated?.Invoke(pos);
        }

        #endregion

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

            //public PositionAccessor UpdatePosition(PositionEntity entity)
            //{
            //    PositionAccessor pos;

            //    if (!_positions.TryGetValue(entity.Symbol, out pos))
            //    {
            //        pos = new PositionAccessor(entity, symbolProvider);
            //        _positions[entity.Symbol] = pos;
            //    }
            //    else
            //        pos.Update(entity);

            //    return pos;
            //}

            //public PositionAccessor GetPosition(string symbol)
            //{
            //    PositionAccessor pos;
            //    _positions.TryGetValue(symbol, out pos);
            //    return pos;
            //}

            public PositionAccessor CreatePosition(SymbolAccessor symbol)
            {
                return _positions.GetOrAdd(symbol.Name, n => new PositionAccessor(symbol));
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

            //public PositionAccessor GetOrCreate(Symbol symbolInfo)
            //{
            //    PositionAccessor pos;

            //    if (!_positions.TryGetValue(symbolInfo.Name, out pos))
            //    {
            //        pos = new PositionAccessor(symbolInfo);
            //        _positions[symbolInfo.Name] = pos;
            //    }

            //    return pos;
            //}

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
