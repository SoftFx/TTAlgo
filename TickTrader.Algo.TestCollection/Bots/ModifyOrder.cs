using System;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.TestCollection.Bots
{
    [TradeBot(DisplayName = "[T] Modify Order Script", Version = "1.3", Category = "Test Orders",
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

        [Parameter(DisplayName = "Expiration Timeout(ms)", DefaultValue = null, IsRequired = false)]
        public int? ExpirationTimeout { get; set; }

        [Parameter(DisplayName = "IoC Flag", DefaultValue = IocTypes.NoneFlag, IsRequired = false)]
        public IocTypes IoC { get; set; }

        protected override void OnStart()
        {
            OrderExecOptions? options = null;
            if (IoC == IocTypes.SetFlag)
                options = OrderExecOptions.ImmediateOrCancel;
            if (IoC == IocTypes.DropFlag)
                options = OrderExecOptions.None;

            ValidateVolume();

            var comment = string.IsNullOrWhiteSpace(Comment) ? null : Comment;

            var result = ModifyOrder(OrderId, Price, StopPrice, MaxVisibleVolume, StopLoss, TakeProfit, comment,
                ExpirationTimeout.HasValue ? DateTime.Now + TimeSpan.FromMilliseconds(ExpirationTimeout.Value) : (DateTime?)null, Volume, options);
            Status.WriteLine($"ResultCode = {result.ResultCode}");
            if (result.ResultingOrder != null)
                Status.WriteLine(ToObjectPropertiesString(typeof(Order), result.ResultingOrder));

            Exit();
        }

        private void ValidateVolume()
        {
            if (Volume.HasValue)
            {
                if (Volume <= 0)
                {
                    Status.WriteLine("Ivalid parameter. Volume cannot be negative.");
                    Exit();
                    throw new Exception("Ivalid parameter. Volume cannot be negative.");
                }
                var order = Account.Orders[OrderId];
                if (order?.IsNull ?? true)
                    return;
                var symbol = Symbols[order.Symbol];
                if (symbol?.IsNull ?? true)
                    return;
                else if (Volume < symbol.MinTradeVolume)
                {
                    Status.WriteLine("Ivalid parameter. Volume is lower than MinTradeVolume.");
                    Exit();
                    throw new Exception("Ivalid parameter. Volume is lower than MinTradeVolume.");
                }
            }
        }
    }

    public enum IocTypes { NoneFlag, DropFlag, SetFlag }
}