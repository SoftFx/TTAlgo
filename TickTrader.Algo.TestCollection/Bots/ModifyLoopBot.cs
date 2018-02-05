using System;
using System.Linq;
using System.Threading.Tasks;
using TickTrader.Algo.Api;
using TickTrader.Algo.Api.Math;

namespace TickTrader.Algo.TestCollection.Bots
{
    [TradeBot(DisplayName = "[T] Modify Loop Bot", Version = "1.0", Category = "Test Orders",
        Description = "")]
    public class ModifyLoopBot : TradeBot
    {
        private int modifyCounter;

        [Parameter(DefaultValue = OrderOperation.Modify)]
        public OrderOperation Operation { get; set; }

        [Parameter(DefaultValue = TestOrderType.Limit)]
        public TestOrderType TestType { get; set; }

        [Parameter(DefaultValue = OrderSide.Buy)]
        public OrderSide TestSide { get; set; }

        [Parameter(DefaultValue = 10)]
        public double ParallelOrders { get; set; }

        [Parameter(DefaultValue = 0.1D)]
        public double Volume { get; set; }

        [Parameter(DefaultValue = null)]
        public double? VolumeChangeMultiplier { get; set; }

        [Parameter(DefaultValue = 1000)]
        public double PriceDelta { get; set; }

        private bool _runningBot = true;

        protected override void OnStart()
        {
            ValidateVolumeChangeMultiplier();

            for (int i = 0; i < ParallelOrders; i++)
            {
                if (Operation == OrderOperation.Modify)
                    ModifyLoop(i, Volume);
                else if (Operation == OrderOperation.OpenCancel)
                    OpenCancelLoop(i, Volume);
            }

            PrintRate();
        }

        private async void PrintRate()
        {
            while (_runningBot)
            {
                await Task.Delay(1000);
                Status.WriteLine("{0} operations per second", modifyCounter);
                modifyCounter = 0;
            }
        }

        private async void ModifyLoop(int orderTag, double orderVolume)
        {
            var strTag = "ModifyLoopBot" + orderTag;

            try
            {
                while (_runningBot)
                {
                    var existing = Account.Orders.FirstOrDefault(o => o.Comment == strTag && o.Type == GetOrderType());

                    if (existing != null)
                        await ModifyOrderAsync(existing.Id, GetNewPrice(), stopPrice: GetNewStopPrice(), volume: GetNewVolume(existing.RequestedVolume), comment: strTag);
                    else
                        await OpenOrderAsync(Symbol.Name, GetOrderType(), GetOrderSide(), Volume, null, GetNewPrice(), stopPrice: GetNewStopPrice(), comment: strTag);

                    modifyCounter++;
                }
            }
            catch(Exception e)
            {
                PrintError(e.Message);
            }
        }

        private async void OpenCancelLoop(int orderTag, double orderVolume)
        {
            var strTag = "ModifyLoopBot" + orderTag;

            while (_runningBot)
            {
                var existing = Account.Orders.FirstOrDefault(o => o.Comment == strTag && o.Type == GetOrderType());

                if (existing != null)
                    await CancelOrderAsync(existing.Id);
                else
                    await OpenOrderAsync(Symbol.Name, GetOrderType(), GetOrderSide(), Volume, null, GetNewPrice(), stopPrice: GetNewStopPrice(), comment: strTag);

                modifyCounter++;
            }
        }

        private double? GetNewPrice()
        {
            if (GetOrderType() == OrderType.Stop)
                return null;

            var ordPrice = GetCurrentPrice();
            if (GetOrderSide() == OrderSide.Buy)
                ordPrice -= Symbol.Point * PriceDelta;
            else
                ordPrice += Symbol.Point * PriceDelta;
            return ordPrice;
        }

        private double? GetNewStopPrice()
        {
            if (GetOrderType() == OrderType.Limit)
                return null;

            var ordPrice = GetCurrentPrice();
            if (GetOrderSide() == OrderSide.Buy)
                ordPrice += Symbol.Point * PriceDelta;
            else
                ordPrice -= Symbol.Point * PriceDelta;
            return ordPrice;
        }

        private double GetNewVolume(double oldVolume)
        {
            if (!VolumeChangeMultiplier.HasValue || VolumeChangeMultiplier.Value == 1 || oldVolume.E(Volume * VolumeChangeMultiplier.Value))
                return Volume;
            else
                return Volume * VolumeChangeMultiplier.Value;
        }

        protected double GetCurrentPrice()
        {

            return GetCurrentPrice(GetOrderSide() == OrderSide.Buy ? BarPriceType.Ask : BarPriceType.Bid);
        }

        private double GetCurrentPrice(BarPriceType type)
        {
            return type == BarPriceType.Ask ? Symbol.Ask : Symbol.Bid;
        }

        private OrderType GetOrderType()
        {
            if (Account.Type == AccountTypes.Cash && TestType == TestOrderType.Stop)
                return OrderType.StopLimit;

            switch (TestType)
            {
                case TestOrderType.Limit:
                    return OrderType.Limit;
                case TestOrderType.Stop:
                    return OrderType.Stop;
                case TestOrderType.StopLimit:
                    return OrderType.StopLimit;
                default:
                    throw new Exception("Invalid testType");
            }
        }

        private OrderSide GetOrderSide()
        {
            return TestSide;
        }

        private void ValidateVolumeChangeMultiplier()
        {
            if( VolumeChangeMultiplier.HasValue &&  VolumeChangeMultiplier <= 0)
            {
                _runningBot = false;
                PrintError("Ivalid value. VolumeChangeMultiplier is negative.");
                Status.WriteLine("Ivalid value. VolumeChangeMultiplier is negative.");
                Exit();
            }
        }

        private async Task CloseAllOrders()
        {
            _runningBot = false;

            for (int i = 0; i < ParallelOrders; i++)
            {
                var ordersForDel = Account.Orders.Where(o => o.Comment == "ModifyLoopBot" + i).Select(o => o.Id).ToList();

                foreach (var orderId in ordersForDel)
                    if (Account.Orders[orderId].Type == OrderType.Position)
                    {
                        if(Account.Type == AccountTypes.Gross)
                            await CloseOrderAsync(orderId);
                    }
                    else
                        await CancelOrderAsync(orderId);
            }
        }

        protected override async Task AsyncStop()
        {
            await CloseAllOrders();

            await base.AsyncStop();
        }
    }

    public enum OrderOperation { Modify, OpenCancel }

    public enum TestOrderType { Limit, Stop, StopLimit }
}
