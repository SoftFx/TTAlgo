using System.Linq;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.TestCollection.Bots
{
    [TradeBot(DisplayName = "[T] Account Info Display Bot", Version = "1.0", Category = "Test Plugin Info",
        Description = "Prints account info to bot status window. This include account id, type, balance, assets, pending orders, positions")]
    public class AccDisplayBot : TradeBot
    {
        private const string GenericDoubleFormat = "0.#########";

        private bool printCalcData;
        private string _baseCurrFormat;

        [Parameter(DefaultValue = AccUpdateTypes.ByTimer)]
        public AccUpdateTypes Refresh { get; set; }

        [Parameter(DefaultValue = BoolEnum.False)]
        public BoolEnum DisplayComments { get; set; }

        protected override void Init()
        {
            if (Account.Type != AccountTypes.Cash)
            {
                var currInfo = Currencies[Account.BalanceCurrency];
                _baseCurrFormat = $"F{currInfo.Digits}";
            }
        }

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

                    Status.Write("#{0} {1} {2:0.#########}/{3:0.#########}", order.Symbol, order.Side,
                        order.RemainingVolume, order.RequestedVolume);
                    if (printCalcData)
                        Status.Write(" margin={0}", order.Margin.ToString(_baseCurrFormat));
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

                    Status.Write("#{0} {1} {2} {3:0.#########}/{4:0.#########}", order.Id, order.Symbol, order.Side,
                        order.RemainingVolume, order.RequestedVolume);
                    if (printCalcData)
                        Status.Write(" margin={0} profit={1}", order.Margin.ToString(_baseCurrFormat), order.Profit.ToString(_baseCurrFormat));
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
                    Status.Write("#{0} {1} {2:0.#########}", pos.Symbol, pos.Side, pos.Volume);
                    if (printCalcData)
                        Status.Write(" margin={0} profit={1}", pos.Margin.ToString(_baseCurrFormat), pos.Profit.ToString(_baseCurrFormat));
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
                    var currFormat = $"F{asset.CurrencyInfo.Digits}";

                    if (asset.CurrencyInfo.IsNull)
                        Status.Write("{0} {1}", asset.Currency, asset.Volume);
                    else
                        Status.Write("{0} {1}", asset.Currency, asset.Volume.ToString(currFormat));

                    Status.Write(" locked={0} free={1}", asset.LockedVolume.ToString(currFormat), asset.FreeVolume.ToString(currFormat));

                    Status.WriteLine();
                }
            }
            else
                Status.WriteLine("No assets");
        }

        private void PrintBalance()
        {
            Status.WriteLine("Balance: {0} {1}", Account.Balance.ToString(_baseCurrFormat), Account.BalanceCurrency);
        }

        private void PrintMarginAccSummary()
        {
            if (printCalcData)
            {
                Status.WriteLine("Equity: {0} {1}", Account.Equity.ToString(_baseCurrFormat), Account.BalanceCurrency);
                Status.WriteLine("Margin: {0} {1}", Account.Margin.ToString(_baseCurrFormat), Account.BalanceCurrency);
                Status.WriteLine("Profit: {0} {1}", Account.Profit.ToString(_baseCurrFormat), Account.BalanceCurrency);
                Status.WriteLine("Margin Level: {0:0.00}%", Account.MarginLevel);
            }
        }
    }

    public enum AccUpdateTypes { ByEvents, ByTimer }
    public enum BoolEnum { True, False }
}
