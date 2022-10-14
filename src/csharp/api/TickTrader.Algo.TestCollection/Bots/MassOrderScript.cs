using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TickTrader.Algo
{
    [TradeBot(Category = "Test Orders", DisplayName = "Mass Order Script", Version = "1.0")]
    public class MassOrderScript : TradeBot
    {
        private int _openedCount;
        private Task[] _workers;
        private Timer _statusTimer;

        [Parameter(DisplayName = "Order Type")]
        public OrderType OrderType { get; set; }

        [Parameter(DisplayName = "Order Volume", DefaultValue = 0.1)]
        public double OrderVolume { get; set; }

        [Parameter(DefaultValue = 10)]
        public int Threads { get; set; }

        [Parameter(DisplayName = "Order Count", DefaultValue = 10)]
        public int OrderCount { get; set; }

        [Parameter(DisplayName = "Price Delta (pips)", DefaultValue = 1000)]
        public int PriceDelta { get; set; }

        [Parameter(DisplayName = "Symbols (csv)")]
        public string SymbolNames { get; set; }

        [Parameter]
        public bool UseExpiration { get; set; }


        protected override void Init()
        {
            if (Threads <= 0)
            {
                PrintError("Threads count must be greater than zero.");
                Exit();
                return;
            }

            if (!PrepareSymbols(out var symbolsToUse))
            {
                Exit();
                return;
            }

            if (OrderType == OrderType.Position)
                OrderType = OrderType.Market;

            _statusTimer = CreateTimer(100, t => UpdateStatus());

            _workers = new Task[Threads];

            for (int i = 0; i < Threads; i++)
                _workers[i] = OpenOrderLoop(symbolsToUse, i);

            HandleCompletion();
        }

        private bool PrepareSymbols(out List<Symbol> symbolsToUse)
        {
            symbolsToUse = new List<Symbol>();

            if (string.IsNullOrWhiteSpace(SymbolNames))
                symbolsToUse.Add(Symbol);
            else
            {
                var parts = SymbolNames.Split(',').Select(s => s.Trim()).ToList();

                foreach (var smbName in parts)
                {
                    var smbInfo = Symbols[smbName];

                    if (smbInfo.IsNull)
                    {
                        PrintError("Symbol '" + smbName + "' does not exsit!");
                        return false;
                    }

                    symbolsToUse.Add(smbInfo);
                }
            }

            return true;
        }

        private async Task OpenOrderLoop(List<Symbol> symbolsToUse, int number)
        {
            var rnd = new Random(UtcNow.Millisecond + number);

            while (_openedCount < OrderCount && !IsStopped)
            {
                _openedCount++;

                var symbol = rnd.Pick(symbolsToUse);
                var side = rnd.PickOne(OrderSide.Buy, OrderSide.Sell);
                var price = PickPrice(symbol, OrderType, side);
                var stopPrice = PickPrice(symbol, OrderType, side);
                var comment = "mass order " + _openedCount;
                DateTime? expiration = null;

                if (UseExpiration)
                    expiration = UtcNow.AddSeconds(rnd.Next());

                await OpenOrderAsync(symbol.Name, OrderType, side, OrderVolume, null, price, stopPrice, null, null, comment, OrderExecOptions.None, null, expiration);
            }
        }

        private double? PickPrice(Symbol smb, OrderType type, OrderSide side)
        {
            if (type == OrderType.Market)
                return (smb.Bid + smb.Ask) / 2;
            else if (type == OrderType.Stop)
            {
                if (side == OrderSide.Buy)
                    return smb.Ask + smb.Point * PriceDelta;
                else
                    return smb.Bid - smb.Point * PriceDelta;
            }
            else if (type == OrderType.Limit || type == OrderType.StopLimit)
            {
                if (side == OrderSide.Buy)
                    return smb.Ask - smb.Point * PriceDelta;
                else
                    return smb.Bid + smb.Point * PriceDelta;
            }

            throw new Exception("Unknown order type: " + OrderType);
        }

        private double? PickStopPrice(Symbol smb, OrderType type, OrderSide side)
        {
            if (type == OrderType.StopLimit)
            {
                if (side == OrderSide.Buy)
                    return smb.Ask + smb.Point * PriceDelta;
                else
                    return smb.Bid - smb.Point * PriceDelta;
            }
            return null;
        }

        private async void HandleCompletion()
        {
            try
            {
                await Task.WhenAll(_workers);
            }
            catch (Exception ex)
            {
                PrintError(ex.ToString());
            }

            _statusTimer?.Dispose();
            UpdateStatus();

            Exit();
        }

        private void UpdateStatus()
        {
            Status.Write("Opened " + _openedCount + " orders.");
        }
    }

    public static class HelperExtentions
    {
        public static T PickOne<T>(this Random rnd, params T[] list)
        {
            return PickCommon(rnd, list);
        }

        public static T Pick<T>(this Random rnd, IList<T> list)
        {
            return PickCommon(rnd, list);
        }

        private static T PickCommon<T>(Random rnd, IList<T> list)
        {
            var index = rnd.Next(0, list.Count);
            return list[index];
        }
    }
}
