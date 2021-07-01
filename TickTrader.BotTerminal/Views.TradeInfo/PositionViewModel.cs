using TickTrader.Algo.Account;
using TickTrader.Algo.Domain;

namespace TickTrader.BotTerminal
{
    internal sealed class PositionViewModel : BaseTransactionViewModel
    {
        private readonly PositionInfo _position;


        public PositionViewModel(PositionInfo position, AccountModel account) : base(account.GetSymbolOrNull(position.Symbol), account.BalanceDigits)
        {
            _position = position;

            Update();
        }

        public override string Id => _position.Id;

        public override double Profit => /*_position?.Calculator?.CalculateProfit(_position) ??*/ 0;


        protected override void Update()
        {
            Modified.Value = _position.Modified?.ToDateTime();

            Price.Value = _position.Price;
            Volume.Value = _position.Volume;

            Side.Value = _position.Side;
            Type.Value = OrderInfo.Types.Type.Position;

            Swap.Value = _position.Swap;
            Commission.Value = _position.Commission;

            RateUpdate(_symbol);
        }
    }
}
