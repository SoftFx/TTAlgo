using System;
using TickTrader.Algo.Api;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.CoreV1
{
    internal sealed class PositionCollection : TradeEntityCollection<PositionAccessor, NetPosition>, NetPositionList
    {
        public PositionCollection(PluginBuilder builder) : base(builder) { }

        public NetPosition this[string symbol] => _entities.TryGetValue(symbol, out PositionAccessor entity) ? entity : new PositionAccessor(symbol, _builder.Symbols.GetOrNull(symbol));

        public PositionAccessor UpdatePosition(PositionInfo posInfo)
        {
            PositionAccessor pos = GetOrCreatePosition(posInfo.Symbol, posInfo.Id);
            pos.Update(posInfo);

            if (posInfo.Volume.Lte(0.0))
                RemovePosition(posInfo.Symbol);

            return pos;
        }

        internal PositionAccessor GetOrCreatePosition(string symbol, string posId)
        {
            var pos = GetOrNull(symbol);

            if (pos == null)
            {
                var smbInfo = _builder.Symbols.GetOrNull(symbol) ?? throw new OrderValidationError($"Symbol Not Found: {symbol}", OrderCmdResultCodes.SymbolNotFound);

                pos = _entities.GetOrAdd(symbol, _ => new PositionAccessor(symbol, _builder.Symbols.GetOrNull(symbol)));
                pos.Info.Id = posId;
                pos.Changed += Pos_Changed;
            }

            return pos;
        }

        internal PositionAccessor RemovePosition(string symbol)
        {
            _entities.TryRemove(symbol, out var toRemove);

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
    }
}
