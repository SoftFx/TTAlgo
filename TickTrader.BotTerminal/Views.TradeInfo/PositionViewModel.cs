using TickTrader.Algo.Common.Model;
using TickTrader.Algo.Domain;

namespace TickTrader.BotTerminal
{
    internal sealed class PositionViewModel : BaseTransactionViewModel
    {
        private readonly PositionInfo _position;

        public PositionViewModel(PositionInfo position, AccountModel account) : base((account as IOrderDependenciesResolver).GetSymbolOrNull(position.Symbol), account.BalanceDigits)
        {
            _position = position;

            Update();
        }

        public override string Id => _position.Id;
        public override decimal Profit => (decimal)(_position?.Calculator?.CalculateProfit(Price.Value.Value, Volume.Value.Value, Side.Value, out _, out _) ?? 0);

        protected override void Update()
        {
            Modified.Value = _position.Modified?.ToDateTime();

            Price.Value = _position.Price;
            Volume.Value = _position.Volume;

            Side.Value = _position.Side;
            Type.Value = Algo.Domain.OrderInfo.Types.Type.Position;

            Swap.Value = (decimal)_position.Swap;
            Commission.Value = (decimal)_position.Commission;
        }
    }
}
