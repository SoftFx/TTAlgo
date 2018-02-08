using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TickTrader.Algo.Core;
using TickTrader.Algo.Api;
using System.Threading.Tasks.Dataflow;
using TickTrader.Algo.Common.Model;
using TickTrader.Algo.Core.Lib;
using Machinarium.Qnil;

namespace TickTrader.BotTerminal
{
    //class TradeHistoryProviderModel
    //{
    //    private static readonly Logger _logger = NLog.LogManager.GetCurrentClassLogger();
    //    private ClientModel.Data _client;

    //    public event Action<TransactionReport> OnTradeReport = delegate { };

    //    public TradeHistoryProviderModel(ClientModel.Data client)
    //    {
    //        _client = client;

    //        //_tradeClient.Connection.Connecting += () => { _tradeClient.Connection.TradeProxy.TradeTransactionReport += TradeTransactionReport; };
    //        //_tradeClient.Connection.Disconnecting += () => { _tradeClient.Connection.TradeProxy.TradeTransactionReport -= TradeTransactionReport; };
    //    }

    //    public async Task<int> DownloadingHistoryAsync(DateTime from, DateTime to, bool skipCancel, CancellationToken token, Action<TransactionReport> reportHandler)
    //    {
    //        token.ThrowIfCancellationRequested();
    //        //var historyStream = _client.TradeHistory.GetTradeHistory(TimeDirection.Forward, true, from, to, 1000, skipCancel);\

    //        var historyStream = _client.TradeHistory.GetTradeHistory(from, to, skipCancel);

    //        //throw new NotImplementedException();

    //        while (await historyStream.ReadNext())
    //        {
    //            if (token.IsCancellationRequested)
    //                break;

    //            foreach (var report in historyStream.Current)
    //            {
    //                var historyItem = TransactionReportFactory.Create(_tradeClient.Account.Type.Value, report, GetSymbolFor(report));
    //                reportHandler(historyItem);
    //            }

    //        }

    //        await historyStream.Close();

    //        return 0;
    //    }

    //    private SymbolModel GetSymbolFor(TradeReportEntity transaction)
    //    {
    //        SymbolModel symbolModel = null;
    //        if (!IsBalanceOperation(transaction))
    //        {
    //            try
    //            {
    //                symbolModel = _client.Symbols.GetOrDefault(transaction.Symbol);
    //            }
    //            catch
    //            {
    //                _logger.Warn("Symbol {0} not found for TradeTransactionID {1}.", transaction.Symbol, transaction.Id);
    //            }
    //        }

    //        return symbolModel;
    //    }

    //    private bool IsBalanceOperation(TradeReportEntity item)
    //    {
    //        return item.TradeTransactionReportType == TradeExecActions.BalanceTransaction;
    //    }

    //    private void TradeTransactionReport(TradeReportEntity report)
    //    {
    //        OnTradeReport(TransactionReportFactory.Create(_client.Cache.Account.Type.Value, report, GetSymbolFor(report)));
    //    }
    //}

    //static class TransactionReportFactory
    //{
    //    public static TransactionReport Create(AccountTypes accountType, TradeReportEntity tTransaction, SymbolModel symbol = null)
    //    {
    //        switch (accountType)
    //        {
    //            case AccountTypes.Gross: return new GrossTransactionModel(tTransaction, symbol);
    //            case AccountTypes.Net: return new NetTransactionModel(tTransaction, symbol);
    //            case AccountTypes.Cash: return new CashTransactionModel(tTransaction, symbol);
    //            default: throw new NotSupportedException(accountType.ToString());
    //        }
    //    }
    //}
}
