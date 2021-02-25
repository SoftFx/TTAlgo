using System;
using System.Threading.Tasks;
using TickTrader.Algo.Core;
using TickTrader.FDK.Common;

namespace TickTrader.Algo.Common.Model.Interop
{
    public class Fdk2FeedHistoryAdapter
    {
        private readonly FDK.Client.QuoteStore _feedHistoryProxy;
        private readonly IAlgoCoreLogger _logger;


        public Fdk2FeedHistoryAdapter(FDK.Client.QuoteStore feedHistoryProxy, IAlgoCoreLogger logger)
        {
            _feedHistoryProxy = feedHistoryProxy;
            _logger = logger;

            _feedHistoryProxy.ConnectResultEvent += (c, d) => SfxTaskAdapter.SetCompleted(d);
            _feedHistoryProxy.ConnectErrorEvent += (c, d, ex) => SfxTaskAdapter.SetFailed(d, ex);

            _feedHistoryProxy.LoginResultEvent += (c, d) => SfxTaskAdapter.SetCompleted(d);
            _feedHistoryProxy.LoginErrorEvent += (c, d, ex) => SfxTaskAdapter.SetFailed(d, ex);

            _feedHistoryProxy.LogoutResultEvent += (c, d, i) => SfxTaskAdapter.SetCompleted(d, i);
            _feedHistoryProxy.LogoutErrorEvent += (c, d, ex) => SfxTaskAdapter.SetFailed<LogoutInfo>(d, ex);

            _feedHistoryProxy.DisconnectResultEvent += (c, d, t) => SfxTaskAdapter.SetCompleted(d, t);

            _feedHistoryProxy.BarListResultEvent += (c, d, r) => SfxTaskAdapter.SetCompleted(d, r);
            _feedHistoryProxy.BarListErrorEvent += (c, d, ex) => SfxTaskAdapter.SetFailed<Bar[]>(d, ex);

            _feedHistoryProxy.QuoteListResultEvent += (c, d, r) => SfxTaskAdapter.SetCompleted(d, r);
            _feedHistoryProxy.QuoteListErrorEvent += (c, d, ex) => SfxTaskAdapter.SetFailed<Bar[]>(d, ex);

            _feedHistoryProxy.HistoryInfoResultEvent += (c, d, r) => SfxTaskAdapter.SetCompleted(d, r);
            _feedHistoryProxy.HistoryInfoErrorEvent += (c, d, ex) => SfxTaskAdapter.SetFailed<HistoryInfo>(d, ex);
        }


        public Task Deinit()
        {
            return Task.Factory.StartNew(() => _feedHistoryProxy.Dispose());
        }


        public Task ConnectAsync(string address)
        {
            var taskSrc = new TaskCompletionSource<object>();
            _feedHistoryProxy.ConnectAsync(taskSrc, address);
            return taskSrc.Task;
        }

        public Task LoginAsync(string username, string password, string deviceId, string appId, string sessionId)
        {
            var taskSrc = new TaskCompletionSource<object>();
            _feedHistoryProxy.LoginAsync(taskSrc, username, password, deviceId, appId, sessionId);
            return taskSrc.Task;
        }

        public Task LogoutAsync(string message)
        {
            var taskSrc = new TaskCompletionSource<LogoutInfo>();
            _feedHistoryProxy.LogoutAsync(taskSrc, message);
            return taskSrc.Task;
        }

        public Task DisconnectAsync(string text)
        {
            var taskSrc = new TaskCompletionSource<string>();
            if (!_feedHistoryProxy.DisconnectAsync(taskSrc, text))
                taskSrc.SetResult("Already disconnected!");
            return taskSrc.Task;
        }

        public async Task<Bar[]> GetBarListAsync(string symbol, PriceType priceType, BarPeriod barPeriod, DateTime from, int count)
        {
            var taskSrc = new SfxTaskAdapter.RequestResultSource<Bar[]>("BarListRequest");
            _feedHistoryProxy.GetBarListAsync(taskSrc, symbol, priceType, barPeriod, from, count);
            var res = await taskSrc.Task;
            _logger.Debug(taskSrc.MeasureRequestTime());
            return res;
        }

        public async Task<Quote[]> GetQuoteListAsync(string symbol, QuoteDepth quoteDepth, DateTime from, int count)
        {
            var taskSrc = new SfxTaskAdapter.RequestResultSource<Quote[]>("QuoteListRequest");
            _feedHistoryProxy.GetQuoteListAsync(taskSrc, symbol, quoteDepth, from, count);
            var res = await taskSrc.Task;
            _logger.Debug(taskSrc.MeasureRequestTime());
            return res;
        }

        public Task<HistoryInfo> GetBarsHistoryInfoAsync(string symbol, BarPeriod period, PriceType priceType)
        {
            var taskSrc = new TaskCompletionSource<HistoryInfo>();
            _feedHistoryProxy.GetBarsHistoryInfoAsync(taskSrc, symbol, period, priceType);
            return taskSrc.Task;
        }

        public Task<HistoryInfo> GetQuotesHistoryInfoAsync(string symbol, bool level2)
        {
            var taskSrc = new TaskCompletionSource<HistoryInfo>();
            _feedHistoryProxy.GetQuotesHistoryInfoAsync(taskSrc, symbol, level2);
            return taskSrc.Task;
        }
    }
}
