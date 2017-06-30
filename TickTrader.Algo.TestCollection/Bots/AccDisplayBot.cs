using System.Linq;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.TestCollection.Bots
{
    [TradeBot(DisplayName = "[T] Account Info Display Bot", Version = "1.0", Category = "Test Plugin Info",
        Description = "Prints account info to bot status window. This include account id, type, balance, assets, pending orders, positions")]
    public class AccDisplayBot : TradeBot
    {
        private bool printCalcData;

        [Parameter(DefaultValue = AccUpdateTypes.ByTimer)]
        public AccUpdateTypes Refresh { get; set; }

        [Parameter(DefaultValue = BoolEnum.False)]
        public BoolEnum DisplayComments { get; set; }

        protected override void OnStart()
        {
            if (Refresh == AccUpdateTypes.ByEvents)
            {
                printCalcData = false;

                PrintStat();

                Account.Orders.Opened += args => PrintStat();
                Account.Orders.Closed += args => PrintStat();
                Account.Orders.Canceled += args => PrintStat();
                Account.Orders.Modified += args => PrintStat();
                Account.Orders.Filled += args => PrintStat();
                Account.Orders.Expired += args => PrintStat();
                Account.Assets.Modified += args => PrintStat();
                Account.BalanceUpdated += () => PrintStat();
            }
            else
            {
                printCalcData = true;
                PrintLoop();
            }
        }

        private async void PrintLoop()
        {
            while (!IsStopped)
            {
                PrintStat();
                await Task.Delay(200);
            }
        }

        private void PrintStat()
        {
            PrintAccountInfo();

            if (Account.Type == AccountTypes.Gross || Account.Type == AccountTypes.Net)
            {
                PrintBalance();
                PrintMarginAccSummary();
            }

            Status.WriteLine();

            if (Account.Type == AccountTypes.Gross)
                PrintGrossPositions();
            else if (Account.Type == AccountTypes.Net)
                PrintNetPositions();
            else if (Account.Type == AccountTypes.Cash)
                PrintAssets();

            Status.WriteLine();

            PrintPendingOrders();
        }

        private void PrintAccountInfo()
        {
            Status.WriteLine("Account: {0} {1}", Account.Id, Account.Type);
        }

        private void PrintPendingOrders()
        {
            var pOrders = Account.Orders.Where(o => o.Type != OrderType.Position).ToList();
            if (pOrders.Count > 0)
            {
                Status.WriteLine("{0}  orders:", pOrders.Count);
                foreach (var order in pOrders)
                {
                    var tag = string.IsNullOrEmpty(order.Tag) ? "" : "[" + order.Tag + "]";

                    Status.Write("#{0} {1} {2}/{3}", order.Symbol, order.Side,
                        order.RemainingVolume, order.RequestedVolume);
                    if (printCalcData)
                        Status.Write(" margin={0:0.00}", order.Margin);
                    if (DisplayComments == BoolEnum.True)
                    {
                        if (!string.IsNullOrEmpty(order.Comment))
                            Status.Write(" comment='" + order.Comment + "'");
                        if (!string.IsNullOrEmpty(order.Tag))
                            Status.Write(" tag='" + order.Tag + "'");
                    }
                    Status.WriteLine();
                }
            }
            else
                Status.WriteLine("No orders");
        }

        private void PrintGrossPositions()
        {
            var positions = Account.Orders.Where(o => o.Type == OrderType.Position).ToList();
            if (positions.Count > 0)
            {
                Status.WriteLine("{0} positions:", positions.Count);
                foreach (var order in positions)
                {
                    var tag = string.IsNullOrEmpty(order.Tag) ? "" : "[" + order.Tag + "]";

                    Status.Write("#{0} {1} {2} {3}/{4}", order.Id, order.Symbol, order.Side,
                        order.RemainingVolume, order.RequestedVolume);
                    if (printCalcData)
                        Status.Write(" margin={0:0.00} profit={1:0.00}", order.Margin, order.Profit);
                    if (DisplayComments == BoolEnum.True)
                    {
                        if (!string.IsNullOrEmpty(order.Comment))
                            Status.Write(" comment='" + order.Comment + "'");
                        if (!string.IsNullOrEmpty(order.Tag))
                            Status.Write(" tag='" + order.Tag + "'");
                    }
                    Status.WriteLine();
                }
            }
            else
                Status.WriteLine("No positions");
        }

        private void PrintNetPositions()
        {
            var positions = Account.NetPositions;

            if (positions.Count > 0)
            {
                Status.WriteLine("{0} positions:", positions.Count);
                foreach (var pos in positions)
                {
                    Status.Write("#{0} {1} {2}", pos.Symbol, pos.Side, pos.Volume);
                    if (printCalcData)
                        Status.Write(" margin={0:0.00} profit={1:0.00}", pos.Margin, pos.Profit);
                    Status.WriteLine();
                }
            }
            else
                Status.WriteLine("No positions");
        }

        private void PrintAssets()
        {
            if (Account.Assets.Count > 0)
            {
                Status.WriteLine("{0} assets:", Account.Assets.Count);
                foreach (var asset in Account.Assets)
                {
                    if (asset.CurrencyInfo.IsNull)
                        Status.Write("{0} {1}", asset.Currency, asset.Volume);
                    else
                        Status.Write("{0} {1}", asset.Currency, asset.Volume.ToString($"F{asset.CurrencyInfo.Digits}"));

                    Status.Write(" locked={0:F2} free={1:F2}", asset.LockedVolume, asset.FreeVolume);

                    Status.WriteLine();
                }
            }
            else
                Status.WriteLine("No assets");
        }

        private void PrintBalance()
        {
            Status.WriteLine("Balance: {0} {1}", Account.Balance, Account.BalanceCurrency);
        }

        private void PrintMarginAccSummary()
        {
            if (printCalcData)
            {
                Status.WriteLine("Equity: {0:0.00} {1}", Account.Equity, Account.BalanceCurrency);
                Status.WriteLine("Margin: {0:0.00} {1}", Account.Margin, Account.BalanceCurrency);
                Status.WriteLine("Profit: {0:0.00} {1}", Account.Profit, Account.BalanceCurrency);
                Status.WriteLine("Margin Level: {0:0.0}%", Account.MarginLevel);
            }
        }
    }

    public enum AccUpdateTypes { ByEvents, ByTimer }
    public enum BoolEnum { True, False }
}
