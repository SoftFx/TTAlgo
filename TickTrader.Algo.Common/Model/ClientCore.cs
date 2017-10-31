using Machinarium.Qnil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Core;

namespace TickTrader.Algo.Common.Model
{
    public class ClientCore
    {
        private ISyncContext _tradeSync;
        private ISyncContext _feedSync;
        private DynamicDictionary<string, CurrencyEntity> _currencies = new DynamicDictionary<string, CurrencyEntity>();

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

                //connection.TradeProxy.AccountInfo += TradeProxy_AccountInfo;
                connection.TradeProxy.ExecutionReport += TradeProxy_ExecutionReport;
                connection.TradeProxy.PositionReport += TradeProxy_PositionReport;
                connection.TradeProxy.TradeTransactionReport += TradeProxy_TradeTransactionReport;
                connection.TradeProxy.BalanceOperation += TradeProxy_BalanceOperation;
            };

            connection.Disconnecting += () =>
            {
                //connection.TradeProxy.AccountInfo -= TradeProxy_AccountInfo;
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
        public IDynamicDictionarySource<string, CurrencyEntity> Currencies => _currencies;
        public ITradeServerApi TradeProxy => Connection.TradeProxy;
        public IFeedServerApi FeedProxy => Connection.FeedProxy;

        public event Action<PositionEntity> PositionReportReceived;
        public event Action<ExecutionReport> ExecutionReportReceived;
        //public event Action<AccountInfoEventArgs> AccountInfoReceived;
        public event Action<TradeReportEntity> TradeTransactionReceived;
        public event Action<BalanceOperationReport> BalanceReceived;
        public event Action<QuoteEntity> TickReceived;

        public void Init()
        {
            _currencies.Clear();
            foreach (var c in FeedProxy.Currencies)
                _currencies.Add(c.Name, c);
            Symbols.Initialize(FeedProxy.Symbols, _currencies.Snapshot);
        }

        public async Task Deinit()
        {
            await Symbols.Deinit();
        }

        private void TradeProxy_PositionReport(PositionEntity position)
        {
            _tradeSync.Invoke(() => PositionReportReceived?.Invoke(position));
        }

        private void TradeProxy_ExecutionReport(ExecutionReport report)
        {
            _tradeSync.Invoke(() => ExecutionReportReceived?.Invoke(report));
        }

        //private void TradeProxy_AccountInfo(object sender, AccountInfoEventArgs e)
        //{
        //    _tradeSync.Invoke(() => AccountInfoReceived?.Invoke(e));
        //}

        private void TradeProxy_TradeTransactionReport(TradeReportEntity report)
        {
            _tradeSync.Invoke(() => TradeTransactionReceived?.Invoke(report));
        }

        private void TradeProxy_BalanceOperation(BalanceOperationReport report)
        {
            _tradeSync.Invoke(() => BalanceReceived?.Invoke(report));
        }

        private void FeedProxy_Tick(QuoteEntity q)
        {
            _tradeSync.Invoke(() => TickReceived?.Invoke(q));
        }
    }
}
