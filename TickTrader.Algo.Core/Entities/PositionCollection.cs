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
        private readonly PluginBuilder _builder;
        private readonly PositionsFixture _fixture;

        internal NetPositionList PositionListImpl { get { return _fixture; } }

        public PositionCollection(PluginBuilder builder)
        {
            _builder = builder;
            _fixture = new PositionsFixture(builder);
        }

        public PositionAccessor UpdatePosition(PositionEntity eReport)
        {
            PositionAccessor pos;

            pos = GetOrCreatePosition(eReport.Symbol, () => eReport.Id);
            pos.Update(eReport);
            if (eReport.Volume <= 0)
                RemovePosition(eReport.Symbol);

            return pos;
        }

        public PositionAccessor GetPositionOrNull(string symbol)
        {
            return _fixture.GetOrDefault(symbol);
        }

        public event Action<PositionAccessor> PositionUpdated;
        //public event Action<PositionAccessor> PositionRemoved;

        public void FirePositionUpdated(NetPositionModifiedEventArgs args)
        {
            _builder.InvokePluginMethod(InvokePositionUpdated, args, false);
        }

        public void FirePositionSplitted(NetPositionSplittedEventArgs args)
        {
            _builder.InvokePluginMethod(InvokePositionSplitted, args, false);
        }

        private void InvokePositionUpdated(PluginBuilder builder, NetPositionModifiedEventArgs args)
        {
            builder.Account.NetPositions._fixture.FirePositionModified(args);
        }

        private void InvokePositionSplitted(PluginBuilder builder, NetPositionSplittedEventArgs args)
        {
            builder.Account.NetPositions._fixture.FirePositionSplitted(args);
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

        internal PositionAccessor GetOrCreatePosition(string symbol, Func<string> idGenerator)
        {
            var pos = _fixture.GetOrDefault(symbol);

            if (pos == null)
            {
                pos = _fixture.CreatePosition(symbol);
                pos.Id = idGenerator();
                pos.Changed += Pos_Changed;
            }

            return pos;
        }

        internal PositionAccessor RemovePosition(string smb)
        {
            var toRemove = _fixture.GetOrDefault(smb);
            if (toRemove != null)
            {
                _fixture.RemovePosition(smb);
                //PositionRemoved?.Invoke(toRemove);
                toRemove.Changed -= Pos_Changed;
            }
            return toRemove;
        }

        private void Pos_Changed(PositionAccessor pos)
        {
            PositionUpdated?.Invoke(pos);
        }

        #endregion

        internal class PositionsFixture : NetPositionList
        {
            private ConcurrentDictionary<string, PositionAccessor> _positions = new ConcurrentDictionary<string, PositionAccessor>();
            private PluginBuilder _builder;

            public PositionsFixture(PluginBuilder builder)
            {
                _builder = builder;
            }

            public NetPosition this[string symbol]
            {
                get
                {
                    PositionAccessor entity;
                    if (!_positions.TryGetValue(symbol, out entity))
                        return PositionAccessor.CreateEmpty(symbol, _builder.Symbols.GetOrDefault, _builder.Account.Leverage);
                    return entity;
                }
            }

            public int Count => _positions.Count;

            internal IEnumerable<PositionAccessor> Values => _positions.Values;

            public event Action<NetPositionModifiedEventArgs> Modified;
            public event Action<NetPositionSplittedEventArgs> Splitted;

            public void FirePositionModified(NetPositionModifiedEventArgs args)
            {
                Modified?.Invoke(args);
            }

            public void FirePositionSplitted(NetPositionSplittedEventArgs args)
            {
                Splitted?.Invoke(args);
            }

            public PositionAccessor CreatePosition(string symbol)
            {
                return _positions.GetOrAdd(symbol, n => new PositionAccessor(symbol, _builder.Symbols.GetOrDefault, _builder.Account.Leverage));
            }

            public bool RemovePosition(string symbol)
            {
                return _positions.TryRemove(symbol, out _);
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
