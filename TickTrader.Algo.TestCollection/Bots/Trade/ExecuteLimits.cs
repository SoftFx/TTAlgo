using System;
using System.Linq;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.TestCollection.Bots
{
    [TradeBot(DisplayName = "[T] Execute limit orders", Version = "1.0", Category = "Test Orders",
        Description = "Execute N limit orders with some basic test")]
    public class ExecuteLimits : TradeBot
    {
        [Parameter(DefaultValue = 1)]
        public double Volume { get; set; }

        [Parameter(DefaultValue = 10)]
        public int NumberOfExecution { get; set; }

        async protected override void OnStart()
        {
            for( int i = 0;i<NumberOfExecution;i++)
            {
                await Test();
            }

            Status.WriteLine("Test was finished.");
            this.Exit();
        }
        async public Task<bool> Test()
        {
            if (Account.Orders.Count != 0)
            {
                Status.WriteLine("Close all orders. Testing was not started");
                this.Exit();
            }

            double currentVolume = TotalNet;
            double expectedVolume = currentVolume + Volume;
            await OpenOrderAsync(this.Symbol.Name, OrderType.Limit, OrderSide.Buy, Volume, Ask + 1000 * Symbol.Point);
            do
            {
                Status.WriteLine("Waiting for limit order execution.");
                if (TotalNet != expectedVolume)
                {
                    Status.WriteLine($"Incorrect total volume. Actual volume is {TotalNet}. Expected = {expectedVolume}");
                    this.Exit();
                    throw new ApplicationException("Incorrect volume was found");
                }
                await Task.Delay(1);
            } while (Account.Orders.Count != 0);

            currentVolume = TotalNet;
            expectedVolume = currentVolume - Volume;
            await OpenOrderAsync(this.Symbol.Name, OrderType.Limit, OrderSide.Sell, Volume, Bid - 1000 * Symbol.Point);
            do
            {
                Status.WriteLine("Waiting for limit order execution.");
                if (TotalNet != expectedVolume)
                {
                    Status.WriteLine($"Incorrect total volume. Actual volume is {TotalNet}. Expected = {expectedVolume}");
                    this.Exit();
                    throw new ApplicationException("Incorrect volume was found");
                }
                await Task.Delay(1);
            } while (Account.Orders.Count != 0);
            return true;
        }

        double TotalNet
        {
            get
            {
                return TotalVolume.BuyVolume - TotalVolume.SellVolume;
            }
        }           

        (double BuyVolume, double SellVolume) TotalVolume
        {
            get
            {
                return (
                    Math.Max(0, Account.NetPositions[Symbol.Name].Volume) + Account.Orders.Where(p => p.Side == OrderSide.Buy).Sum(p => p.RemainingVolume),
                    -Math.Min(0, Account.NetPositions[Symbol.Name].Volume) + Account.Orders.Where(p => p.Side == OrderSide.Sell).Sum(p => p.RemainingVolume)
                    );
            }
        }

    }
}
