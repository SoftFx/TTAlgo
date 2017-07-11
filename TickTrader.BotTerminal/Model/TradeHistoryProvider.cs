using NLog;
using SoftFX.Extended;
using SoftFX.Extended.Reports;
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

namespace TickTrader.BotTerminal
{
    class TradeHistoryProvider : CrossDomainObject, ITradeHistoryProvider
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

                var historyStream = _tradeClient.Connection.TradeProxy.Server.GetTradeTransactionReports(TimeDirection.Forward, true, from, to, 1000, false);

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

        public IAsyncCrossDomainEnumerator<Algo.Api.TradeReport> GetTradeHistory()
        {
            return GetTradeHistoryInternal(null, null);
        }

        public IAsyncCrossDomainEnumerator<Algo.Api.TradeReport> GetTradeHistory(DateTime from, DateTime to)
        {
            return GetTradeHistoryInternal(from, to);
        }

        public IAsyncCrossDomainEnumerator<Algo.Api.TradeReport> GetTradeHistory(DateTime to)
        {
            return GetTradeHistoryInternal(null, to);
        }

        private IAsyncCrossDomainEnumerator<Algo.Api.TradeReport> GetTradeHistoryInternal(DateTime? from, DateTime? to)
        {
            return new StreamDownloader(_tradeClient.Connection.TradeProxy.Server, from, to);
        }

        private class StreamDownloader : CrossDomainObject, IAsyncEnumerator<TradeReport>, IAsyncCrossDomainEnumerator<TradeReport>
        {
            private BufferBlock<object> _asyncBlock;
            private Task _downloadTask;
            private CancellationTokenSource _stopSrc = new CancellationTokenSource();

            public StreamDownloader(DataTradeServer server, DateTime? from, DateTime? to)
            {
                var asynBlockOptions = new DataflowBlockOptions() { BoundedCapacity = 2, CancellationToken = _stopSrc.Token };
                _asyncBlock = new BufferBlock<object>(asynBlockOptions);

                _downloadTask = Task.Run(() =>
                {
                    const int bufferSize = 500;
                    List<TradeReport> pageBuffer = new List<TradeReport>(bufferSize);
                    StreamIterator<TradeTransactionReport> stream = null;

                    try
                    {
                        stream = server.GetTradeTransactionReports(TimeDirection.Backward, true, from, to, 1000, false);

                        while (!stream.EndOfStream && !_stopSrc.Token.IsCancellationRequested)
                        {
                            var report = FdkToAlgo.Convert(stream.Item);
                            pageBuffer.Add(report);

                            if (pageBuffer.Count == bufferSize)
                            {
                                _asyncBlock.SendAsync(pageBuffer.ToArray(), _stopSrc.Token).Wait();
                                pageBuffer.Clear();
                            }

                            stream.Next();
                        }

                        if (pageBuffer.Count > 0 && !_stopSrc.IsCancellationRequested)
                            _asyncBlock.SendAsync(pageBuffer.ToArray());

                        _asyncBlock.SendAsync(null);
                    }
                    catch (Exception ex)
                    {
                        var aggeEx = ex as AggregateException;
                        if (aggeEx == null || !(aggeEx.InnerException is TaskCanceledException))
                            _asyncBlock.SendAsync(ex);
                    }
                    _asyncBlock.Complete();
                    if (stream != null)
                        stream.Dispose();
                });
            }

            public Task<TradeReport[]> GetNextPage()
            {
                return _asyncBlock.ReceiveAsync().ContinueWith(t =>
                    {
                        if (t.Result is Exception)
                            throw (Exception)t.Result;
                        return (TradeReport[])t.Result;
                    });
            }

            public override void Dispose()
            {
                _stopSrc.Cancel();
                _downloadTask.Wait();
                try
                {
                    _asyncBlock.Completion.Wait();
                }
                catch (Exception) { };

                base.Dispose();
            }

            public void GetNextPage(CrossDomainTaskProxy<TradeReport[]> pageCallback)
            {
                _asyncBlock.ReceiveAsync().ContinueWith(t =>
                {
                    if (t.Result is Exception)
                        pageCallback.SetException((Exception)t.Result);
                    pageCallback.SetResult((TradeReport[])t.Result);
                });
            }
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
