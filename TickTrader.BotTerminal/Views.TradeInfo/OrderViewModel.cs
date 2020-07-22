using Caliburn.Micro;
using Machinarium.Var;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;
using TickTrader.Algo.Common;
using TickTrader.Algo.Common.Model;
using TickTrader.Algo.Core;
using TickTrader.Algo.Domain;
using TickTrader.BotTerminal.Converters;

namespace TickTrader.BotTerminal
{
    sealed class OrderViewModel : IDisposable
    {
        private readonly VarContext _varContext = new VarContext();
        private readonly PricePrecisionConverter<double?> _symbolPrecision;
        private readonly PricePrecisionConverter<decimal?> _profitPrecision;
        private readonly AmountToLotsConverter<double?> _amountToLots;

        private readonly OrderModel _order;
        private readonly SymbolModel _symbol;

        public OrderViewModel(OrderModel order, SymbolModel symbol, int accountDigits)
        {
            _order = order;
            _symbol = symbol;

            _symbolPrecision = new PricePrecisionConverter<double?>(order?.SymbolInfo?.Digits ?? 5);
            _profitPrecision = new PricePrecisionConverter<decimal?>(accountDigits);
            _amountToLots = new AmountToLotsConverter<double?>(_symbol.LotSize);

            Price = _varContext.AddProperty(order.Price, _symbolPrecision);
            StopPrice = _varContext.AddProperty(order.StopPrice, _symbolPrecision);
            LimitPrice = _varContext.AddProperty(order.LimitPrice, _symbolPrecision);
            ReqOpenPrice = _varContext.AddProperty(order.ReqOpenPrice, _symbolPrecision);
            DeviationPrice = _varContext.AddProperty(displayConverter: _symbolPrecision);

            Volume = _varContext.AddProperty(order.Amount, _amountToLots);
            RemainingVolume = _varContext.AddProperty(order.RemainingAmount, _amountToLots);
            MaxVisibleVolume = _varContext.AddProperty(order.MaxVisibleAmount, _amountToLots);

            Type = _varContext.AddProperty(order.Type);
            InitType = _varContext.AddProperty(order.InitialType);
            Side = _varContext.AddProperty(order.Side);
            AggregatedType = _varContext.AddProperty(order.Side.Aggregate(Type.Value));

            Swap = _varContext.AddProperty(order.Swap);
            Commission = _varContext.AddProperty(order.Commission, _profitPrecision);
            NetProfit = _varContext.AddProperty(displayConverter: _profitPrecision);

            Created = _varContext.AddProperty(order.Created);

            Expiration = _varContext.AddProperty(order.Expiration);
            Modified = _varContext.AddProperty(order.Modified);

            Comment = _varContext.AddProperty(order.Comment);
            Tag = _varContext.AddProperty(order.UserTag);
            ParentOrderId = _varContext.AddProperty(order.ParentOrderId);
            InstanceId = _varContext.AddProperty(order.InstanceId);
            OrderExecutionOptions = _varContext.AddProperty(order.ExecOptions.GetString());

            TakeProfit = _varContext.AddProperty(order.TakeProfit);
            StopLoss = _varContext.AddProperty(order.StopLoss);
            Slippage = _varContext.AddProperty(order.Slippage);

            order.EssentialsChanged += o => Update();

            if (order.SymbolInfo != null) // server misconfiguration can cause unexisting symbols
                order.SymbolInfo.RateUpdated += RateUpdate;
        }

        public string Id => _order.Id; //use on UI
        public string Symbol => _order.Symbol; //use on UI
        public string SortedNumber => $"{_order.Modified?.ToString("dd.MM.yyyyHH:mm:ss.fff")}-{_order.Id}"; //use on UI for correct sorting

        public bool IsBuy => _order.Side == Algo.Domain.OrderInfo.Types.Side.Buy;
        public bool IsPosition => _order.Type != Algo.Domain.OrderInfo.Types.Type.Position;

        public RateDirectionTracker CurrentPrice => IsPosition ? IsBuy ? _symbol?.AskTracker : _symbol?.BidTracker :
                                                                 IsBuy ? _symbol?.BidTracker : _symbol?.AskTracker;

        public Property<double?> Price { get; }
        public Property<double?> StopPrice { get; }
        public Property<double?> LimitPrice { get; }
        public Property<double?> ReqOpenPrice { get; }
        public Property<double?> DeviationPrice { get; }

        public Property<double?> Volume { get; }
        public Property<double?> RemainingVolume { get; }
        public Property<double?> MaxVisibleVolume { get; }

        public Property<Algo.Domain.OrderInfo.Types.Type> Type { get; }
        public Property<Algo.Domain.OrderInfo.Types.Type> InitType { get; }
        public Property<Algo.Domain.OrderInfo.Types.Side> Side { get; }
        public Property<AggregatedOrderType> AggregatedType { get; }

        public Property<decimal?> Swap { get; }
        public Property<decimal?> Commission { get; }
        public Property<decimal?> NetProfit { get; }

        public Property<DateTime?> Created { get; }
        public Property<DateTime?> Expiration { get; }
        public Property<DateTime?> Modified { get; }

        public Property<string> Comment { get; }
        public Property<string> Tag { get; }
        public Property<string> OrderExecutionOptions { get; }
        public Property<string> ParentOrderId { get; }
        public Property<string> InstanceId { get; }

        public Property<double?> TakeProfit { get; }
        public Property<double?> StopLoss { get; }
        public Property<double?> Slippage { get; }

        private void Update()
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
            //NetProfit.Value = _order.NetProfit;
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

        private void RateUpdate(ISymbolInfo2 symbols)
        {
            DeviationPrice.Value = IsBuy ? CurrentPrice?.Rate - Price.Value : Price.Value - CurrentPrice?.Rate;
            NetProfit.Value = Swap.Value + Commission.Value + (decimal)_order?.Calculator.CalculateProfit(_order, out _);
        }

        public void Dispose()
        {
            if (_order.SymbolInfo != null) // server misconfiguration can cause unexisting symbols
                _order.SymbolInfo.RateUpdated -= RateUpdate;
        }
    }
}
