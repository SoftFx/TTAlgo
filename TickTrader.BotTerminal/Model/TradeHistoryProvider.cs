using NLog;
using SoftFX.Extended;
using SoftFX.Extended.Reports;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TickTrader.BotTerminal
{
    class TradeHistoryProvider
    {
        private static readonly Logger _logger = NLog.LogManager.GetCurrentClassLogger();
        private TraderClientModel _tradeClient;

        public event Action<TransactionReport> OnTradeReport = delegate { };

        public TradeHistoryProvider(TraderClientModel connectionModel)
        {
            _tradeClient = connectionModel;

            _tradeClient.Connection.Connecting += () => { _tradeClient.Connection.TradeProxy.TradeTransactionReport += TradeTransactionReport; };
            _tradeClient.Connection.Disconnecting += () => { _tradeClient.Connection.TradeProxy.TradeTransactionReport -= TradeTransactionReport; };
        }

        public Task<int> DownloadingHistoryAsync(DateTime from, DateTime to, CancellationToken token, Action<TransactionReport> reportHandler)
        {
            return Task.Run(() =>
            {
                token.ThrowIfCancellationRequested();

                var historyStream = _tradeClient.Connection.TradeProxy.Server.GetTradeTransactionReports(TimeDirection.Forward, true, from, to, 1000);

                while (!historyStream.EndOfStream)
                {
                    token.ThrowIfCancellationRequested();

                    var historyItem = TransactionReportFactory.Create(_tradeClient.Account.Type.Value, historyStream.Item, GetSymbolFor(historyStream.Item));
                    reportHandler(historyItem);

                    historyStream.Next();
                }

                return 0;
            }, token);
        }

        private SymbolModel GetSymbolFor(TradeTransactionReport transaction)
        {
            SymbolModel symbolModel = null;
            if (!IsBalanceOperation(transaction))
            {
                try
                {
                    symbolModel = (SymbolModel)_tradeClient.Symbols.GetOrDefault(transaction.Symbol);
                }
                catch
                {
                    _logger.Warn("Symbol {0} not found for TradeTransactionID {1}.", transaction.Symbol, transaction.Id);
                }
            }

            return symbolModel;
        }

        private bool IsBalanceOperation(TradeTransactionReport item)
        {
            return item.TradeTransactionReportType == TradeTransactionReportType.BalanceTransaction;
        }

        private void TradeTransactionReport(object sender, SoftFX.Extended.Events.TradeTransactionReportEventArgs e)
        {
            OnTradeReport(TransactionReportFactory.Create(_tradeClient.Account.Type.Value, e.Report, GetSymbolFor(e.Report)));
        }
    }

    static class TransactionReportFactory
    {
        public static TransactionReport Create(AccountType accountType, TradeTransactionReport tTransaction, SymbolModel symbol = null)
        {
            switch(accountType)
            {
                case AccountType.Gross: return new GrossTransactionModel(tTransaction, symbol);
                case AccountType.Net: return new NetTransactionModel(tTransaction, symbol);
                case AccountType.Cash: return new CashTransactionModel(tTransaction, symbol);
                default: throw new NotSupportedException(accountType.ToString());
            }
        }
    }
}
