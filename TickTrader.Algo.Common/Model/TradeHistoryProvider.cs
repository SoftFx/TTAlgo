using SoftFX.Extended;
using SoftFX.Extended.Reports;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using TickTrader.Algo.Api;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Lib;

namespace TickTrader.Algo.Common.Model
{
    public class TradeHistoryProvider : CrossDomainObject, ITradeHistoryProvider
    {
        private ConnectionModel _connection;

        public TradeHistoryProvider(ConnectionModel connection)
        {
            _connection = connection;
        }

        public IAsyncCrossDomainEnumerator<TradeReport> GetTradeHistory(bool skipCancelOrders)
        {
            return GetTradeHistoryInternal(null, null, skipCancelOrders);
        }

        public IAsyncCrossDomainEnumerator<TradeReport> GetTradeHistory(DateTime from, DateTime to, bool skipCancelOrders)
        {
            return GetTradeHistoryInternal(from, to, skipCancelOrders);
        }

        public IAsyncCrossDomainEnumerator<TradeReport> GetTradeHistory(DateTime to, bool skipCancelOrders)
        {
            return GetTradeHistoryInternal(null, to, skipCancelOrders);
        }

        private IAsyncCrossDomainEnumerator<TradeReport> GetTradeHistoryInternal(DateTime? from, DateTime? to, bool skipCancelOrders)
        {
            return new StreamDownloader(_connection.TradeProxy.Server, from, to, skipCancelOrders);
        }

        private class StreamDownloader : CrossDomainObject, IAsyncCrossDomainEnumerator<TradeReport>
        {
            private BufferBlock<object> _asyncBlock;
            private Task _downloadTask;
            private CancellationTokenSource _stopSrc = new CancellationTokenSource();

            public StreamDownloader(DataTradeServer server, DateTime? from, DateTime? to, bool skipCancelOrders)
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
                        stream = server.GetTradeTransactionReports(TimeDirection.Backward, true, from, to, 1000, skipCancelOrders);

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
}
