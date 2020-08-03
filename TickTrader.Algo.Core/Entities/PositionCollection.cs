using System;
using TickTrader.Algo.Api;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Core
{
    internal sealed class PositionCollection : TradeEntityCollection<PositionAccessor, Api.NetPosition>, Api.NetPositionList
    {
        public PositionCollection(PluginBuilder builder) : base(builder) { }

        public NetPosition this[string symbol] => !_entities.TryGetValue(symbol, out PositionAccessor entity) ? entity : new PositionAccessor(_builder.Symbols.GetOrDefault(symbol));

        public PositionAccessor UpdatePosition(PositionInfo posInfo)
        {
            PositionAccessor pos = GetOrCreatePosition(posInfo.Symbol, posInfo.Id);
            pos.Update(posInfo);

            if (posInfo.Volume <= 0)
                RemovePosition(posInfo.Symbol);

            return pos;
        }

        internal PositionAccessor GetOrCreatePosition(string symbol, string posId)
        {
            var pos = GetOrNull(symbol);

            if (pos == null)
            {
                var smbInfo = _builder.Symbols.GetOrDefault(symbol) ?? throw new OrderValidationError("Symbol Not Found:  " + symbol, OrderCmdResultCodes.SymbolNotFound);

                pos = _entities.GetOrAdd(symbol, _ => new PositionAccessor(smbInfo));
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
