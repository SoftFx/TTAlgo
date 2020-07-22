using TickTrader.Algo.Common.Model;

namespace TickTrader.BotTerminal
{
    internal sealed class PositionViewModel : BaseTransactionViewModel
    {
        private readonly PositionModel _position;

        public PositionViewModel(PositionModel position, int accountDigits) : base(position?.SymbolModel, accountDigits)
        {
            _position = position;

            Update();
        }

        public override string Id => _position.Id;
        public override decimal Profit => (decimal)(_position?.Calculator?.CalculateProfit(Price.Value.Value, Volume.Value.Value, Side.Value, out _, out _) ?? 0);

        protected override void Update()
        {
            Modified.Value = _position.Modified;

            Price.Value = _position.Price;
            Volume.Value = _position.Amount;

            Side.Value = _position.Side;
            Type.Value = Algo.Domain.OrderInfo.Types.Type.Position;

            Swap.Value = _position.Swap;
            Commission.Value = _position.Commission;
        }
    }
}
