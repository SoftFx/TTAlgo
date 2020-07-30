using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using TickTrader.Algo.Api;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Core
{
    public sealed class PositionCollection : NetPositionList
    {
        private readonly ConcurrentDictionary<string, PositionAccessor> _positions = new ConcurrentDictionary<string, PositionAccessor>();
        private readonly PluginBuilder _builder;

        public PositionCollection(PluginBuilder builder)
        {
            _builder = builder;
        }

        public int Count => _positions.Count;

        public void Clear() => _positions.Clear();

        public IEnumerable<PositionAccessor> Values => _positions.Values;

        public PositionAccessor GetPositionOrNull(string symbol) => _positions.GetOrDefault(symbol);

        public NetPosition this[string symbol] => !_positions.TryGetValue(symbol, out PositionAccessor entity) ? entity : new PositionAccessor(_builder.Symbols.GetOrDefault(symbol));

        public PositionAccessor UpdatePosition(Domain.PositionInfo posInfo)
        {
            PositionAccessor pos = GetOrCreatePosition(posInfo.Symbol, posInfo.Id);
            pos.Update(posInfo);

            if (posInfo.Volume <= 0)
                RemovePosition(posInfo.Symbol);

            return pos;
        }

        internal PositionAccessor GetOrCreatePosition(string symbol, string posId)
        {
            var pos = GetPositionOrNull(symbol);

            if (pos == null)
            {
                var smbInfo = _builder.Symbols.GetOrDefault(symbol) ?? throw new OrderValidationError("Symbol Not Found:  " + symbol, OrderCmdResultCodes.SymbolNotFound);

                pos = _positions.GetOrAdd(symbol, _ => new PositionAccessor(smbInfo));
                pos.Info.Id = posId;
                pos.Changed += Pos_Changed;
            }

            return pos;
        }

        internal PositionAccessor RemovePosition(string symbol)
        {
            _positions.TryRemove(symbol, out var toRemove);

            if (toRemove != null)
                toRemove.Changed -= Pos_Changed;

            return toRemove;
        }

        public event Action<IPositionInfo> PositionUpdated;
        public event Action<NetPositionModifiedEventArgs> Modified;
        public event Action<NetPositionSplittedEventArgs> Splitted;

        public void FirePositionUpdated(NetPositionModifiedEventArgs args) => _builder.InvokePluginMethod((b, p) => Modified?.Invoke(p), args, false);

        public void FirePositionSplitted(NetPositionSplittedEventArgs args) => _builder.InvokePluginMethod((b, p) => Splitted?.Invoke(p), args, false);

        private void Pos_Changed(PositionAccessor pos) => PositionUpdated?.Invoke(pos.Info);

        IEnumerator<NetPosition> IEnumerable<NetPosition>.GetEnumerator() => _positions.Values.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _positions.Values.GetEnumerator();
    }
}
