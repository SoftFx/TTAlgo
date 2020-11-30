﻿using System;
using System.Linq;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.TestCollection.Bots
{
    [TradeBot(DisplayName = "[T] Modify Order Script", Version = "1.4", Category = "Test Orders",
        SetupMainSymbol = false, Description = "Modifies order by OrderId. Prints order execution result to bot status window.")]
    public class ModifyOrder : TradeBotCommon
    {
        [Parameter(DefaultValue = "")]
        public string OrderId { get; set; }

        [Parameter(DefaultValue = null)]
        public double? Price { get; set; }

        [Parameter(DefaultValue = null)]
        public double? StopPrice { get; set; }

        [Parameter(DefaultValue = null)]
        public double? Volume { get; set; }

        [Parameter(DefaultValue = null)]
        public double? MaxVisibleVolume { get; set; }

        [Parameter(DefaultValue = "")]
        public string Comment { get; set; }

        [Parameter(DisplayName = "Stop Loss", DefaultValue = null, IsRequired = false)]
        public double? StopLoss { get; set; }

        [Parameter(DisplayName = "Take Profit", DefaultValue = null, IsRequired = false)]
        public double? TakeProfit { get; set; }

        [Parameter(DisplayName = "Slippage (fraction)", DefaultValue = null, IsRequired = false)]
        public double? Slippage { get; set; }

        [Parameter(DisplayName = "Expiration Timeout(ms)", DefaultValue = null, IsRequired = false)]
        public int? ExpirationTimeout { get; set; }

        [Parameter(DisplayName = "IoC Flag", DefaultValue = IocTypes.DoNotModify, IsRequired = false)]
        public IocTypes IoC { get; set; }

        protected override void OnStart()
        {
            OrderExecOptions? options = null;
            if (IoC == IocTypes.IoC)
                options = OrderExecOptions.ImmediateOrCancel;
            if (IoC == IocTypes.None)
                options = OrderExecOptions.None;

            var comment = string.IsNullOrWhiteSpace(Comment) ? null : Comment;

            if (string.IsNullOrWhiteSpace(OrderId))
                OrderId = Account.Orders.FirstOrDefault()?.Id;

            var request = ModifyOrderRequest.Template.Create().WithOrderId(OrderId)
                .WithPrice(Price).WithStopPrice(StopPrice).WithMaxVisibleVolume(MaxVisibleVolume)
                .WithStopLoss(StopLoss).WithTakeProfit(TakeProfit).WithComment(comment)
                .WithExpiration(ExpirationTimeout.HasValue ? DateTime.Now + TimeSpan.FromMilliseconds(ExpirationTimeout.Value) : (DateTime?)null)
                .WithVolume(Volume).WithOptions(options).WithSlippage(Slippage).MakeRequest();

            var result = ModifyOrder(request);

            Status.WriteLine($"ResultCode = {result.ResultCode}");
            if (result.ResultingOrder != null)
                Status.WriteLine(ToObjectPropertiesString(result.ResultingOrder));

            Exit();
        }
    }

    public enum IocTypes { DoNotModify, None, IoC }
}