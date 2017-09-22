using Machinarium.Qnil;
using SoftFX.Extended;
using SoftFX.Extended.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Common.Model
{
    public class ClientCore
    {
        private ISyncContext _tradeSync;
        private ISyncContext _feedSync;
        private DynamicDictionary<string, CurrencyInfo> _currencies = new DynamicDictionary<string, CurrencyInfo>();

        public ClientCore(ConnectionModel connection,
            Func<ClientCore, SymbolCollectionBase> symbolCollectionFactory,
            ISyncContext tradeSync, ISyncContext feedSync)
        {
            Connection = connection;
            Symbols = symbolCollectionFactory(this);
            Distributor = Symbols.Distributor;
            _tradeSync = tradeSync;
            _feedSync = feedSync;

            connection.Connecting += () =>
            {
                connection.FeedProxy.Tick += FeedProxy_Tick;

                connection.TradeProxy.AccountInfo += TradeProxy_AccountInfo;
                connection.TradeProxy.ExecutionReport += TradeProxy_ExecutionReport;
                connection.TradeProxy.PositionReport += TradeProxy_PositionReport;
                connection.TradeProxy.TradeTransactionReport += TradeProxy_TradeTransactionReport;
                connection.TradeProxy.BalanceOperation += TradeProxy_BalanceOperation;
            };

            connection.Disconnecting += () =>
            {
                connection.TradeProxy.AccountInfo -= TradeProxy_AccountInfo;
                connection.TradeProxy.ExecutionReport -= TradeProxy_ExecutionReport;
                connection.TradeProxy.PositionReport -= TradeProxy_PositionReport;
                connection.TradeProxy.TradeTransactionReport -= TradeProxy_TradeTransactionReport;
                connection.TradeProxy.BalanceOperation -= TradeProxy_BalanceOperation;
            };
        }

        internal ISyncContext FeedSync => _feedSync;
        internal ISyncContext TradeSync => _tradeSync;

        public ConnectionModel Connection { get; }
        public QuoteDistributor Distributor { get; }
        public SymbolCollectionBase Symbols { get; }
        public IDynamicDictionarySource<string, CurrencyInfo> Currencies => _currencies;
        public DataTrade TradeProxy => Connection.TradeProxy;
        public DataFeed FeedProxy => Connection.FeedProxy;

        public event Action<PositionReportEventArgs> PositionReportReceived;
        public event Action<ExecutionReportEventArgs> ExecutionReportReceived;
        public event Action<AccountInfoEventArgs> AccountInfoReceived;
        public event Action<TradeTransactionReportEventArgs> TradeTransactionReceived;
        public event Action<NotificationEventArgs<BalanceOperation>> BalanceReceived;
        public event Action<TickEventArgs> TickReceived;

        public void Init()
        {
            var cache = FeedProxy.Cache;
            _currencies.Clear();
            foreach (var c in cache.Currencies)
                _currencies.Add(c.Name, c);
            Symbols.Initialize(FeedProxy.Cache.Symbols, _currencies.Snapshot);
        }

        public async Task Deinit()
        {
            await Symbols.Deinit();
        }

        private void TradeProxy_PositionReport(object sender, PositionReportEventArgs e)
        {
            _tradeSync.Invoke(() => PositionReportReceived?.Invoke(e));
        }

        private void TradeProxy_ExecutionReport(object sender, ExecutionReportEventArgs e)
        {
            _tradeSync.Invoke(() => ExecutionReportReceived?.Invoke(e));
        }

        private void TradeProxy_AccountInfo(object sender, AccountInfoEventArgs e)
        {
            _tradeSync.Invoke(() => AccountInfoReceived?.Invoke(e));
        }

        private void TradeProxy_TradeTransactionReport(object sender, TradeTransactionReportEventArgs e)
        {
            _tradeSync.Invoke(() => TradeTransactionReceived?.Invoke(e));
        }

        private void TradeProxy_BalanceOperation(object sender, NotificationEventArgs<BalanceOperation> e)
        {
            _tradeSync.Invoke(() => BalanceReceived?.Invoke(e));
        }

        private void FeedProxy_Tick(object sender, TickEventArgs e)
        {
            _tradeSync.Invoke(() => TickReceived?.Invoke(e));
        }
    }
}
