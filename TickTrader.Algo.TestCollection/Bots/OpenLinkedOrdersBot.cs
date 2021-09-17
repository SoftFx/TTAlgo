﻿using System;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.TestCollection.Bots
{
    [TradeBot(DisplayName = "[T] Open Linked Orders Script", Version = "1.0", Category = "Test Orders")]
    public class OpenLinkedOrdersBot : TradeBotCommon
    {
        private const string MainOrderComment = "Main Linked Order";
        private const string LinkedOrderComment = "Linked Order";


        [Parameter(DefaultValue = 1.0)]
        public double? Price { get; set; }

        [Parameter]
        public double? StopPrice { get; set; }

        [Parameter(DefaultValue = 1)]
        public double Volume { get; set; }

        [Parameter]
        public OrderSide Side { get; set; }

        [Parameter(DefaultValue = OrderType.Limit)]
        public OrderType Type { get; set; }

        [Parameter(DefaultValue = OrderExecOptions.OneCancelsTheOther)]
        public OrderExecOptions Options { get; set; }

        [Parameter(DefaultValue = null, DisplayName = "Expiration timeout (s)")]
        public int? ExpirationTimeout { get; set; }


        [Parameter(DefaultValue = 1.0)]
        public double? PriceSecond { get; set; }

        [Parameter]
        public double? StopPriceSecond { get; set; }

        [Parameter(DefaultValue = 1)]
        public double VolumeSecond { get; set; }

        [Parameter]
        public OrderSide SideSecond { get; set; }

        [Parameter(DefaultValue = OrderType.Limit)]
        public OrderType TypeSecond { get; set; }

        [Parameter(DefaultValue = null, DisplayName = "ExpirationSecond timeout (s)")]
        public int? ExpirationTimeout2 { get; set; }


        [Parameter]
        public ContingentOrderTrigger.TriggerType OtoType { get; set; }

        [Parameter(DefaultValue = null, DisplayName = "OTO timeout(sec)")]
        public int? OtoTimeTimeout { get; set; }

        [Parameter]
        public string OtoTriggeredById { get; set; }


        protected override void Init()
        {
            Account.Orders.Opened += OrderCollectionEvent;
            Account.Orders.Filled += OrderCollectionEvent;
            Account.Orders.Closed += OrderCollectionEvent;
            Account.Orders.Expired += OrderCollectionEvent;
            Account.Orders.Activated += OrderCollectionEvent;
            Account.Orders.Canceled += OrderCollectionEvent;
            Account.Orders.Splitted += OrderCollectionEvent;
            Account.Orders.Modified += OrderCollectionEvent;
        }

        protected async override void OnStart()
        {
            var linkedOrder = OpenOrderRequest.Template.Create()
                .WithParams(Symbol.Name, SideSecond, TypeSecond, VolumeSecond, PriceSecond, StopPriceSecond, LinkedOrderComment)
                .WithExpiration(GetDateTime(ExpirationTimeout2))
                .WithTag(nameof(OpenLinkedOrdersBot));

            var otoTrigger = ContingentOrderTrigger.Create(OtoType, GetDateTime(OtoTimeTimeout), OtoTriggeredById);

            var mainOrder = OpenOrderRequest.Template.Create()
                .WithParams(Symbol.Name, Side, Type, Volume, Price, StopPrice, MainOrderComment)
                .WithExpiration(GetDateTime(ExpirationTimeout))
                .WithOptions(Options)
                .WithTag(nameof(OpenLinkedOrdersBot))
                .WithSubOpenRequests(linkedOrder.MakeRequest())
                .WithContingentOrderTrigger(otoTrigger);

            var answer = await OpenOrderAsync(mainOrder.MakeRequest());

            if (answer.ResultCode == OrderCmdResultCodes.Ok)
                Print(ToObjectPropertiesString("MainOrderResult", answer.ResultingOrder));
            else
                Exit();
        }

        private static DateTime? GetDateTime(int? timeout)
        {
            if (timeout.HasValue)
                return DateTime.Now + TimeSpan.FromSeconds(timeout.Value);

            return null;
        }

        protected override void OnStop()
        {
            Account.Orders.Opened -= OrderCollectionEvent;
            Account.Orders.Filled -= OrderCollectionEvent;
            Account.Orders.Closed -= OrderCollectionEvent;
            Account.Orders.Expired -= OrderCollectionEvent;
            Account.Orders.Activated -= OrderCollectionEvent;
            Account.Orders.Canceled -= OrderCollectionEvent;
            Account.Orders.Splitted -= OrderCollectionEvent;
            Account.Orders.Modified -= OrderCollectionEvent;
        }

        private void OrderCollectionEvent<T>(T obj)
        {
            Print($"Event received: {typeof(T).Name}");

            if (obj is SingleOrderEventArgs single)
                Print(ToObjectPropertiesString(nameof(single.Order), single.Order));

            if (obj is DoubleOrderEventArgs doubleOrder)
            {
                Print(ToObjectPropertiesString(nameof(doubleOrder.OldOrder), doubleOrder.OldOrder));
                Print(ToObjectPropertiesString(nameof(doubleOrder.NewOrder), doubleOrder.NewOrder));
            }
        }
    }
}