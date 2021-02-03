using System.Threading.Tasks;
using TickTrader.Algo.Core;
using TickTrader.FDK.Common;
using FDK2 = TickTrader.FDK.Common;

namespace TickTrader.Algo.Common.Model.Interop
{
    public class Fdk2FeedAdapter
    {
        private readonly FDK.Client.QuoteFeed _feedProxy;
        private readonly IAlgoCoreLogger _logger;

        public Fdk2FeedAdapter(FDK.Client.QuoteFeed feedProxy, IAlgoCoreLogger logger)
        {
            _feedProxy = feedProxy;
            _logger = logger;

            _feedProxy.ConnectResultEvent += (c, d) => SfxTaskAdapter.SetCompleted(d);
            _feedProxy.ConnectErrorEvent += (c, d, ex) => SfxTaskAdapter.SetFailed(d, ex);

            _feedProxy.LoginResultEvent += (c, d) => SfxTaskAdapter.SetCompleted(d);
            _feedProxy.LoginErrorEvent += (c, d, ex) => SfxTaskAdapter.SetFailed(d, ex);

            _feedProxy.LogoutResultEvent += (c, d, i) => SfxTaskAdapter.SetCompleted(d, i);
            _feedProxy.LogoutErrorEvent += (c, d, ex) => SfxTaskAdapter.SetFailed<LogoutInfo>(d, ex);

            _feedProxy.DisconnectResultEvent += (c, d, t) => SfxTaskAdapter.SetCompleted(d, t);

            _feedProxy.QuotesResultEvent += (c, d, r) => SfxTaskAdapter.SetCompleted(d, SfxInterop.Convert(r));
            _feedProxy.QuotesErrorEvent += (c, d, ex) => SfxTaskAdapter.SetFailed<QuoteEntity[]>(d, ex);

            _feedProxy.CurrencyListResultEvent += (c, d, r) => SfxTaskAdapter.SetCompleted(d, r);
            _feedProxy.CurrencyListErrorEvent += (c, d, ex) => SfxTaskAdapter.SetFailed<FDK2.CurrencyInfo[]>(d, ex);

            _feedProxy.SymbolListResultEvent += (c, d, r) => SfxTaskAdapter.SetCompleted(d, r);
            _feedProxy.SymbolListErrorEvent += (c, d, ex) => SfxTaskAdapter.SetFailed<FDK2.SymbolInfo[]>(d, ex);

            _feedProxy.SubscribeQuotesResultEvent += (c, d, r) => SfxTaskAdapter.SetCompleted(d, SfxInterop.Convert(r));
            _feedProxy.SubscribeQuotesErrorEvent += (c, d, ex) => SfxTaskAdapter.SetFailed<QuoteEntity[]>(d, ex);

            _feedProxy.QuotesResultEvent += (c, d, r) => SfxTaskAdapter.SetCompleted(d, SfxInterop.Convert(r));
            _feedProxy.QuotesErrorEvent += (c, d, ex) => SfxTaskAdapter.SetFailed<QuoteEntity[]>(d, ex);
        }


        public Task Deinit()
        {
            return Task.Factory.StartNew(() => _feedProxy.Dispose());
        }


        public Task ConnectAsync(string address)
        {
            var taskSrc = new TaskCompletionSource<object>();
            _feedProxy.ConnectAsync(taskSrc, address);
            return taskSrc.Task;
        }

        public Task LoginAsync(string username, string password, string deviceId, string appId, string sessionId)
        {
            var taskSrc = new TaskCompletionSource<object>();
            _feedProxy.LoginAsync(taskSrc, username, password, deviceId, appId, sessionId);
            return taskSrc.Task;
        }

        public Task LogoutAsync(string message)
        {
            var taskSrc = new TaskCompletionSource<LogoutInfo>();
            _feedProxy.LogoutAsync(taskSrc, message);
            return taskSrc.Task;
        }

        public Task DisconnectAsync(string text)
        {
            var taskSrc = new TaskCompletionSource<string>();
            if (!_feedProxy.DisconnectAsync(taskSrc, text))
                taskSrc.SetResult("Already disconnected!");
            return taskSrc.Task;
        }

        public async Task<FDK2.CurrencyInfo[]> GetCurrencyListAsync()
        {
            var taskSrc = new SfxTaskAdapter.RequestResultSource<FDK2.CurrencyInfo[]>("CurrencyListRequest");
            _feedProxy.GetCurrencyListAsync(taskSrc);
            var res = await taskSrc.Task;
            _logger.Debug(taskSrc.MeasureRequestTime());
            return res;
        }

        public async Task<FDK2.SymbolInfo[]> GetSymbolListAsync()
        {
            var taskSrc = new SfxTaskAdapter.RequestResultSource<FDK2.SymbolInfo[]>("SymbolListRequest");
            _feedProxy.GetSymbolListAsync(taskSrc);
            var res = await taskSrc.Task;
            _logger.Debug(taskSrc.MeasureRequestTime());
            return res;
        }

        public async Task<QuoteEntity[]> SubscribeQuotesAsync(string[] symbolIds, int marketDepth)
        {
            var taskSrc = new SfxTaskAdapter.RequestResultSource<QuoteEntity[]>("SubscribeQuotesRequest");
            _feedProxy.SubscribeQuotesAsync(taskSrc, SfxConvert.GetSymbolEntries(symbolIds, marketDepth));
            var res = await taskSrc.Task;
            _logger.Debug(taskSrc.MeasureRequestTime());
            return res;
        }

        public async Task<Quote[]> GetQuotesAsync(string[] symbolIds, int marketDepth)
        {
            var taskSrc = new SfxTaskAdapter.RequestResultSource<Quote[]>("GetQuotesRequest");
            _feedProxy.GetQuotesAsync(taskSrc, SfxConvert.GetSymbolEntries(symbolIds, marketDepth));
            var res = await taskSrc.Task;
            _logger.Debug(taskSrc.MeasureRequestTime());
            return res;
        }
    }
}
