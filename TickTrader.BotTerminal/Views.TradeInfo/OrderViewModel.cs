using Machinarium.Var;
using System;
using TickTrader.Algo.Common.Model;
using TickTrader.Algo.Core;
using TickTrader.Algo.Domain;

namespace TickTrader.BotTerminal
{
    internal sealed class OrderViewModel : BaseTransactionViewModel
    {
        private readonly OrderInfo _order;

        public OrderViewModel(OrderInfo order, SymbolInfo symbol, int accountDigits) : base(symbol, accountDigits)
        {
            _order = order;

            StopPrice = _varContext.AddProperty(displayConverter: _symbolPrecision);
            LimitPrice = _varContext.AddProperty(displayConverter: _symbolPrecision);
            ReqOpenPrice = _varContext.AddProperty(displayConverter: _symbolPrecision);

            RemainingVolume = _varContext.AddProperty(displayConverter: _amountToLots);
            MaxVisibleVolume = _varContext.AddProperty(displayConverter: _amountToLots);

            InitType = _varContext.AddProperty(order.InitialType);

            AggregatedType = _varContext.AddProperty(order.Side.Aggregate(Type.Value));

            Created = _varContext.AddProperty(order.Created?.ToDateTime());
            Expiration = _varContext.AddProperty(order.Expiration?.ToDateTime());

            Comment = _varContext.AddProperty(order.Comment);
            Tag = _varContext.AddProperty(order.UserTag);
            ParentOrderId = _varContext.AddProperty(order.ParentOrderId);
            InstanceId = _varContext.AddProperty(order.InstanceId);
            OrderExecutionOptions = _varContext.AddProperty(order.Options.GetString());

            TakeProfit = _varContext.AddProperty(order.TakeProfit);
            StopLoss = _varContext.AddProperty(order.StopLoss);
            Slippage = _varContext.AddProperty(order.Slippage);

            order.EssentialsChanged += o => Update();

            Update();
        }

        public override string Id => _order.Id;
        public override double Profit => _order?.Calculator != null ? (double)_order?.Calculator?.CalculateProfit(_order.IsStopOrder ? _order.StopPrice.Value : _order.Price.Value, (double)_order.RemainingAmount, _order.Side, out _, out _) : 0;

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
            Price.Value = _order.Type == OrderInfo.Types.Type.Stop || _order.Type == OrderInfo.Types.Type.StopLimit ? _order.StopPrice : _order.Price;
            StopPrice.Value = _order.StopPrice;
            LimitPrice.Value = _order.Type == OrderInfo.Types.Type.Limit || _order.Type == OrderInfo.Types.Type.StopLimit ? _order.Price : 0;
            ReqOpenPrice.Value = _order.RequestedOpenPrice;

            Volume.Value = _order.RequestedAmount;
            RemainingVolume.Value = _order.RemainingAmount;
            MaxVisibleVolume.Value = _order.MaxVisibleAmount;

            Type.Value = _order.Type;
            InitType.Value = _order.InitialType;
            Side.Value = _order.Side;
            AggregatedType.Value = _order.Side.Aggregate(Type.Value);

            Swap.Value = _order.Swap;
            Commission.Value = _order.Commission;

            Created.Value = _order.Created?.ToDateTime();
            Expiration.Value = _order.Expiration?.ToDateTime();
            Modified.Value = _order.Modified?.ToDateTime();

            Comment.Value = _order.Comment;
            OrderExecutionOptions.Value = _order.Options.ToString();
            ParentOrderId.Value = _order.ParentOrderId;

            if (CompositeTag.TryParse(_order.UserTag, out CompositeTag compositeTag))
            {
                Tag.Value = compositeTag.Tag;
                InstanceId.Value = compositeTag.Key;
            }
            else
            {
                Tag.Value = _order.UserTag;
                InstanceId.Value = "";
            }

            TakeProfit.Value = _order.TakeProfit;
            StopLoss.Value = _order.StopLoss;
            Slippage.Value = _order.Slippage;
        }
    }
}
