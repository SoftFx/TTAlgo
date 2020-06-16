using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;
using TickTrader.Algo.Api.Math;

namespace TickTrader.Algo.TestCollection.Auto.Tests
{
    internal class HistoryOrderTemplate : OrderTemplate
    {
        ////public string OrderId { get; }
        ////public AccountTypes AccType { get; }
        ////public OrderType InitialType { get; }
        //public OrderType CurrentType { get; }
        ////public OrderSide Side { get; }
        //public double ReqVolume { get; }
        //public double RemVolume { get; set; }
        ////public double? Price { get; }
        ////public double? StopPrice { get; }

        public TradeExecActions TradeReportAction { get; protected set; }

        public DateTime TradeReportTimestamp { get; protected set; }

        public OrderType TradeRecordType { get; protected set; }

        protected HistoryOrderTemplate(OrderTemplate template)
        {
            Id = template.Id;
            Side = template.Side;
            Type = template.Type;
            Volume = template.Volume;
            Price = template.Price;
            StopPrice = template.StopPrice;
            TP = template.TP;
            SL = template.SL;
            Options = template.Options;

            InitOpenPrice = template.InitOpenPrice;
        }

        protected static HistoryOrderTemplate Create(OrderTemplate template, OrderFilledEventArgs args) =>
            new HistoryOrderTemplate(template)
            {
                TradeReportAction = TradeExecActions.OrderFilled,
                TradeReportTimestamp = args.OldOrder.Modified
            };

        protected static HistoryOrderTemplate Create(OrderTemplate template, OrderActivatedEventArgs args) =>
            new HistoryOrderTemplate(template)
            {
                TradeReportAction = TradeExecActions.OrderActivated,
                TradeReportTimestamp = args.Order.Modified
            };

        protected static HistoryOrderTemplate Create(OrderTemplate template, OrderClosedEventArgs args) =>
            new HistoryOrderTemplate(template)
            {
                TradeReportAction = TradeExecActions.PositionClosed,
                TradeReportTimestamp = args.Order.Modified
            };

        protected static HistoryOrderTemplate Create(OrderTemplate template, OrderCanceledEventArgs args) =>
            new HistoryOrderTemplate(template)
            {
                TradeReportAction = TradeExecActions.OrderCanceled,
                TradeReportTimestamp = args.Order.Modified
            };

        protected static HistoryOrderTemplate Create(OrderTemplate template, OrderExpiredEventArgs args) =>
            new HistoryOrderTemplate(template)
            {
                TradeReportAction = TradeExecActions.OrderExpired,
                TradeReportTimestamp = args.Order.Modified
            };

        public static HistoryOrderTemplate Create(OrderTemplate template, object args)
        {
            if (args is OrderFilledEventArgs)
                return Create(template, (OrderFilledEventArgs)args);
            if (args is OrderActivatedEventArgs)
                return Create(template, (OrderActivatedEventArgs)args);
            if (args is OrderClosedEventArgs)
                return Create(template, (OrderClosedEventArgs)args);
            if (args is OrderCanceledEventArgs)
                return Create(template, (OrderCanceledEventArgs)args);
            if (args is OrderExpiredEventArgs)
                return Create(template, (OrderExpiredEventArgs)args);

            throw new Exception("UnknownType");
        }

        public void VerifyTradeReport(TradeReport report)
        {
            HistoryVerificationException.StorageId = Id;
            HistoryVerificationException.HistoryId = Type == OrderType.Position ? report.PositionId : report.OrderId;

            if (Id != HistoryVerificationException.HistoryId)
                throw new HistoryVerificationException();

            AssertEquals(nameof(report.ActionType), TradeReportAction, report.ActionType);
            AssertEquals(nameof(report.TradeRecordSide), Side, report.TradeRecordSide);
            AssertEquals(nameof(report.TradeRecordType), Type, report.TradeRecordType);

            if (TradeReportAction != TradeExecActions.OrderCanceled && TradeReportAction != TradeExecActions.PositionClosed) //redone
                AssertEqualsDouble(nameof(report.ReqOpenPrice), TradeReportAction == TradeExecActions.OrderActivated || Type == OrderType.Stop ? StopPrice.Value : Price.Value, report.ReqOpenPrice.Value);
        }

        private void AssertEquals<T>(string property, T current, T expected)
        {
            if (!expected.Equals(current))
                throw new HistoryVerificationException(property, current, expected);
        }

        private void AssertEqualsDouble(string property, double current, double expected)
        {
            if (!expected.E(current))
                throw new HistoryVerificationException(property, current, expected);
        }
    }

    public class VerificationException : Exception
    {
        public VerificationException(string id, bool close) :
            base($"{GetTitle(id)} {(close ? "still exist in order collection!" : "does not exist in order collection!")}")
        { }


        public VerificationException(string id, string property, object req, object cur) :
            base($"{GetTitle(id)} has wrong {property}: required = {req}, current = {cur}")
        { }


        private static string GetTitle(string id) => $"Verification failed - order #{id}";
    }

    public class HistoryVerificationException : Exception
    {
        public static string StorageId { get; set; }

        public static string HistoryId { get; set; }

        public HistoryVerificationException() : base($"Error Storage id={StorageId} VS History id={HistoryId}") { }

        public HistoryVerificationException(string property, object cur, object req) :
            base($"Error Storage id={StorageId} {property}={cur} VS History id={HistoryId} {property}={req}")
        { }
    }
}
