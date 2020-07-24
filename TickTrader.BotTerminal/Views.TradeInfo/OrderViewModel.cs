using Machinarium.Var;
using System;
using TickTrader.Algo.Common.Model;
using TickTrader.Algo.Domain;

namespace TickTrader.BotTerminal
{
    internal sealed class OrderViewModel : BaseTransactionViewModel
    {
        private readonly OrderModel _order;

        public OrderViewModel(OrderModel order, SymbolInfo symbol, int accountDigits) : base(symbol, accountDigits)
        {
            _order = order;

            StopPrice = _varContext.AddProperty(displayConverter: _symbolPrecision);
            LimitPrice = _varContext.AddProperty(displayConverter: _symbolPrecision);
            ReqOpenPrice = _varContext.AddProperty(displayConverter: _symbolPrecision);

            RemainingVolume = _varContext.AddProperty(displayConverter: _amountToLots);
            MaxVisibleVolume = _varContext.AddProperty(displayConverter: _amountToLots);

            InitType = _varContext.AddProperty(order.InitialType);

            AggregatedType = _varContext.AddProperty(order.Side.Aggregate(Type.Value));

            Created = _varContext.AddProperty(order.Created);
            Expiration = _varContext.AddProperty(order.Expiration);

            Comment = _varContext.AddProperty(order.Comment);
            Tag = _varContext.AddProperty(order.UserTag);
            ParentOrderId = _varContext.AddProperty(order.ParentOrderId);
            InstanceId = _varContext.AddProperty(order.InstanceId);
            OrderExecutionOptions = _varContext.AddProperty(order.ExecOptions.GetString());

            TakeProfit = _varContext.AddProperty(order.TakeProfit);
            StopLoss = _varContext.AddProperty(order.StopLoss);
            Slippage = _varContext.AddProperty(order.Slippage);

            order.EssentialsChanged += o => Update();

            Update();
        }

        public override string Id => _order.Id;
        public override decimal Profit => _order?.Calculator != null ? (decimal)_order?.Calculator?.CalculateProfit(_order, out _) : 0M;

        public Property<double?> StopPrice { get; }
        public Property<double?> LimitPrice { get; }
        public Property<double?> ReqOpenPrice { get; }

        public Property<double?> RemainingVolume { get; }
        public Property<double?> MaxVisibleVolume { get; }

        public Property<Algo.Domain.OrderInfo.Types.Type> InitType { get; }
        public Property<AggregatedOrderType> AggregatedType { get; }

        public Property<DateTime?> Created { get; }
        public Property<DateTime?> Expiration { get; }

        public Property<string> Comment { get; }
        public Property<string> Tag { get; }
        public Property<string> OrderExecutionOptions { get; }
        public Property<string> ParentOrderId { get; }
        public Property<string> InstanceId { get; }

        public Property<double?> TakeProfit { get; }
        public Property<double?> StopLoss { get; }
        public Property<double?> Slippage { get; }

        protected override void Update()
        {
            Price.Value = _order.Price;
            StopPrice.Value = _order.StopPrice;
            LimitPrice.Value = _order.LimitPrice;
            ReqOpenPrice.Value = _order.ReqOpenPrice;

            Volume.Value = _order.Amount;
            RemainingVolume.Value = _order.RemainingAmount;
            MaxVisibleVolume.Value = _order.MaxVisibleAmount;

            Type.Value = _order.Type;
            InitType.Value = _order.InitialType;
            Side.Value = _order.Side;
            AggregatedType.Value = _order.Side.Aggregate(Type.Value);

            Swap.Value = _order.Swap;
            Commission.Value = _order.Commission;

            Created.Value = _order.Created;
            Expiration.Value = _order.Expiration;
            Modified.Value = _order.Modified;

            Comment.Value = _order.Comment;
            Tag.Value = _order.UserTag;
            OrderExecutionOptions.Value = _order.ExecOptions.ToString();
            ParentOrderId.Value = _order.ParentOrderId;
            InstanceId.Value = _order.InstanceId;

            TakeProfit.Value = _order.TakeProfit;
            StopLoss.Value = _order.StopLoss;
            Slippage.Value = _order.Slippage;
        }
    }
}
