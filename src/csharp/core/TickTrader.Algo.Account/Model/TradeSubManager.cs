using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TickTrader.Algo.Core.Subscriptions;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Account
{
    public class TradeSubManager
    {
        private readonly static TimeSpan DefaultSubTimeout = TimeSpan.FromMinutes(1);
        private readonly static int DefaultSubDepth = SubscriptionDepth.Tick_S0;

        private readonly AccountModel _acc;
        private readonly IQuoteSub _sub;
        private readonly Dictionary<string, SymbolTradesInfo> _bySymbols = new Dictionary<string, SymbolTradesInfo>();

        private Task _monitorTask;
        private CancellationTokenSource _monitoringCancelSrc;


        public TradeSubManager(AccountModel acc, IQuoteSub sub)
        {
            _acc = acc;
            _sub = sub;

            _acc.OrdersAdded += OnOrdersAdded;
            _acc.OrderAdded += OnOrderAdded;
            _acc.OrderRemoved += OnOrderRemoved;
            _acc.PositionChanged += OnPositionChanged;
            _acc.PositionRemoved += OnPositionRemoved;
        }


        public void Dispose()
        {
            _acc.OrdersAdded -= OnOrdersAdded;
            _acc.OrderAdded -= OnOrderAdded;
            _acc.OrderRemoved -= OnOrderRemoved;
            _acc.PositionChanged -= OnPositionChanged;
            _acc.PositionRemoved -= OnPositionRemoved;

            _sub.Dispose();
        }


        public void Start()
        {
            if (_monitoringCancelSrc != null)
                return;

            _monitoringCancelSrc = new CancellationTokenSource();
            _monitorTask = MonitorLoop(_monitoringCancelSrc.Token);
        }

        public async Task Stop()
        {
            if (_monitoringCancelSrc == null)
                return;

            _monitoringCancelSrc.Cancel();
            _monitoringCancelSrc = null;
            try
            {
                await _monitorTask;
            }
            finally
            {
                Reset();
            }
        }


        private async Task MonitorLoop(CancellationToken cancelToken)
        {
            try
            {
                while (cancelToken.IsCancellationRequested)
                {
                    ResetOutdated();
                    await Task.Delay(1000, cancelToken);
                }
            }
            catch (OperationCanceledException) { }
        }


        private void Reset()
        {
            foreach(var tradesInfo in _bySymbols.Values)
            {
                tradesInfo.OrdersCnt = 0;
                tradesInfo.HasPosition = false;
                tradesInfo.SubTimeout = DateTime.MinValue;
                CheckResetSub(tradesInfo);
            }
        }

        private void ResetOutdated()
        {
            foreach (var tradesInfo in _bySymbols.Values)
            {
                CheckResetSub(tradesInfo);
            }
        }


        private void OnOrderAdded(IOrderCalcInfo order)
        {
            var tradesInfo = GetOrAddTradesInfo(order.Symbol);
            tradesInfo.OrdersCnt++;
            AddSub(tradesInfo);
        }

        private void OnOrdersAdded(IEnumerable<IOrderCalcInfo> orders)
        {
            foreach (var order in orders)
                OnOrderAdded(order);
        }

        private void OnOrderRemoved(IOrderCalcInfo order)
        {
            var tradesInfo = GetOrAddTradesInfo(order.Symbol);
            tradesInfo.OrdersCnt--;
            if (tradesInfo.OrdersCnt < 0)
            {
                tradesInfo.OrdersCnt = 0; // reset to zero in case smth goes wrong
            }
            SetSubTimeout(tradesInfo);
        }

        private void OnPositionChanged(IPositionInfo position)
        {
            var tradesInfo = GetOrAddTradesInfo(position.Symbol);
            tradesInfo.HasPosition = true;
            AddSub(tradesInfo);
        }

        private void OnPositionRemoved(IPositionInfo position)
        {
            var tradesInfo = GetOrAddTradesInfo(position.Symbol);
            tradesInfo.HasPosition = false;
            SetSubTimeout(tradesInfo);
        }


        private SymbolTradesInfo GetOrAddTradesInfo(string symbol)
        {
            if (!_bySymbols.TryGetValue(symbol, out var tradesInfo))
            {
                tradesInfo = new SymbolTradesInfo { Symbol = symbol };
                _bySymbols[symbol] = tradesInfo;
            }

            return tradesInfo;
        }

        private void AddSub(SymbolTradesInfo info)
        {
            if (!info.Subscribed)
            {
                _sub.Modify(info.Symbol, DefaultSubDepth);
                info.Subscribed = true;
            }
        }

        private void SetSubTimeout(SymbolTradesInfo info)
        {
            if (!info.HasTrades)
            {
                info.SubTimeout = DateTime.UtcNow + DefaultSubTimeout;
            }
        }

        private void CheckResetSub(SymbolTradesInfo info)
        {
            if (info.Subscribed && !info.HasTrades && info.SubTimeout < DateTime.UtcNow)
            {
                _sub.Modify(info.Symbol, SubscriptionDepth.RemoveSub);
                info.Subscribed = false;
            }
        }


        private class SymbolTradesInfo
        {
            public string Symbol { get; set; }

            public DateTime SubTimeout { get; set; } = DateTime.MinValue;

            public int OrdersCnt { get; set; }

            public bool HasPosition { get; set; }

            public bool HasTrades => HasPosition || OrdersCnt > 0;

            public bool Subscribed { get; set; }
        }
    }
}
