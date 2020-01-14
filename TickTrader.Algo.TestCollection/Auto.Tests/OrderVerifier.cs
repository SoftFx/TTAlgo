using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.TestCollection.Auto.Tests
{
    internal class OrderVerifier
    {
        public OrderVerifier(string orderId, AccountTypes accType, OrderType initialType, OrderSide side, double volume, double? price, double? stopPrice, DateTime trTimestamp)
            : this(orderId, accType, initialType, initialType, side, volume, volume, price, stopPrice, TradeExecActions.None, trTimestamp)
        {
        }

        public OrderVerifier(string orderId, AccountTypes accType, OrderType initialType, OrderType currentType, OrderSide side,
            double reqVolume, double remVolume, double? price, double? stopPrice, TradeExecActions trAction, DateTime trTimestamp)
        {
            OrderId = orderId;
            AccType = accType;
            InitialType = initialType;
            CurrentType = currentType;
            Side = side;
            ReqVolume = reqVolume;
            RemVolume = remVolume;
            Price = price;
            StopPrice = stopPrice;
            TradeReportAction = trAction;
            TradeReportTimestamp = trTimestamp;
        }

        public string OrderId { get; }
        public AccountTypes AccType { get; }
        public OrderType InitialType { get; }
        public OrderType CurrentType { get; }
        public OrderSide Side { get; }
        public double ReqVolume { get; }
        public double RemVolume { get; }
        public double? Price { get; }
        public double? StopPrice { get; }
        public TradeExecActions TradeReportAction { get; }
        public DateTime TradeReportTimestamp { get; }

        #region Order lifetime tracking

        public OrderVerifier Fill(DateTime execTime)
        {
            if (AccType == AccountTypes.Gross)
                return Clone(OrderType.Position, ReqVolume, TradeExecActions.OrderFilled, execTime);
            else
                return Clone(CurrentType, 0, TradeExecActions.OrderFilled, execTime);
        }

        public OrderVerifier Activate(DateTime execTime)
        {
            return Clone(CurrentType, RemVolume, TradeExecActions.OrderActivated, execTime);
        }

        public OrderVerifier Close(DateTime execTime)
        {
            return Clone(CurrentType, 0, TradeExecActions.PositionClosed, execTime);
        }

        public OrderVerifier Close(double amount, DateTime execTime)
        {
            var newVolume = ReqVolume - amount;
            return Clone(CurrentType, newVolume, TradeExecActions.PositionClosed, execTime);
        }

        public OrderVerifier Cancel(DateTime execTime)
        {
            return Clone(CurrentType, RemVolume, TradeExecActions.OrderCanceled, execTime);
        }

        public OrderVerifier Expire(DateTime execTime)
        {
            return Clone(CurrentType, RemVolume, TradeExecActions.OrderExpired, execTime);
        }

        public OrderVerifier Clone(OrderType newType, double newRemVolume, TradeExecActions action, DateTime trTimestamp)
        {
            return new OrderVerifier(OrderId, AccType, InitialType, newType, Side, ReqVolume, newRemVolume, Price, StopPrice, action, trTimestamp);
        }

        #endregion

        #region Events Verification

        public void VerifyEvent(OrderFilledEventArgs args)
        {
            AssertEquals(OrderId, args.NewOrder.Id, $"OrderId does not match!");
            AssertEquals(CurrentType, args.NewOrder.Type, $"CurrentType does not match! {CurrentType} vs {args.NewOrder.Type}");
            AssertEquals(Side, args.NewOrder.Side, $"Side does not match!");
            AssertEquals(ReqVolume, args.NewOrder.RequestedVolume, $"ReqVolume does not match!");
            AssertEquals(RemVolume, args.NewOrder.RemainingVolume, $"RemVolume does not match!");
            AssertEquals(ReqVolume, args.NewOrder.LastFillVolume, $"FillVolume does not match!");
        }

        public void VerifyEvent(OrderActivatedEventArgs args)
        {
            AssertEquals(OrderId, args.Order.Id, $"OrderId does not match!");
            AssertEquals(CurrentType, args.Order.Type, $"CurrentType does not match!");
            AssertEquals(Side, args.Order.Side, $"Side does not match!");
            AssertEquals(ReqVolume, args.Order.RequestedVolume, $"ReqVolume does not match!");
            AssertEquals(RemVolume, args.Order.RemainingVolume, $"RemVolume does not match!");
        }

        public void VerifyEvent(OrderClosedEventArgs args)
        {
            AssertEquals(OrderId, args.Order.Id, $"OrderId does not match!");
            AssertEquals(CurrentType, args.Order.Type, $"CurrentType does not match!");
            AssertEquals(Side, args.Order.Side, $"Side does not match!");
            AssertEquals(ReqVolume, args.Order.RequestedVolume, $"ReqVolume does not match!");
            AssertEquals(RemVolume, args.Order.RemainingVolume, $"RemVolume does not match!");
        }

        public void VerifyEvent(OrderCanceledEventArgs args)
        {
            AssertEquals(OrderId, args.Order.Id, $"OrderId does not match!");
            AssertEquals(CurrentType, args.Order.Type, $"CurrentType does not match!");
            AssertEquals(Side, args.Order.Side, $"Side does not match!");
            AssertEquals(ReqVolume, args.Order.RequestedVolume, $"ReqVolume does not match!");
            AssertEquals(RemVolume, args.Order.RemainingVolume, $"RemVolume does not match!");
        }

        public void VerifyEvent(OrderExpiredEventArgs args)
        {
            AssertEquals(OrderId, args.Order.Id, $"OrderId does not match!");
            AssertEquals(CurrentType, args.Order.Type, $"CurrentType does not match!");
            AssertEquals(Side, args.Order.Side, $"Side does not match!");
            AssertEquals(ReqVolume, args.Order.RequestedVolume, $"ReqVolume does not match!");
            AssertEquals(RemVolume, args.Order.RemainingVolume, $"RemVolume does not match!");
        }

        #endregion

        #region Verification

        public void VerifyTradeReport(TradeReport report)
        {
            AssertEquals(OrderId, report.OrderId, $"OrderId does not match! {OrderId} vs {report.OrderId}");
            AssertEquals(TradeReportAction, report.ActionType, $"ActionType does not match! {TradeReportAction} vs {report.ActionType}, Id = {OrderId}");
            AssertEquals(ReqVolume, report.OpenQuantity, $"OpenQuantity does not match! {ReqVolume} vs {report.OpenQuantity}, Id = {OrderId}");
            AssertEquals(RemVolume, report.RemainingQuantity, $"RemainingQuantity does not match! {RemVolume} vs {report.RemainingQuantity}, Id = {OrderId}");
        }

        private void AssertEquals<T>(T expected, T actual, string message)
        {
            if (!expected.Equals(actual))
                throw new Exception(message);
        }

        private void Assert(Func<bool> condition, string message)
        {
            if (!condition())
                throw new Exception(message);
        }

        #endregion
    }
}
