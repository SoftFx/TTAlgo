using ActorSharp;
using System;
using System.Threading.Tasks;
using TickTrader.Algo.Core;
using TickTrader.FDK.Common;

namespace TickTrader.Algo.Common.Model.Interop
{
    public class Fdk2TradeHistoryAdapter
    {
        private readonly FDK.Client.TradeCapture _tradeCapture;
        private readonly IAlgoCoreLogger _logger;


        public Fdk2TradeHistoryAdapter(FDK.Client.TradeCapture tradeCapture, IAlgoCoreLogger logger)
        {
            _tradeCapture = tradeCapture;
            _logger = logger;

            _tradeCapture.ConnectResultEvent += (c, d) => SfxTaskAdapter.SetCompleted(d);
            _tradeCapture.ConnectErrorEvent += (c, d, ex) => SfxTaskAdapter.SetFailed(d, ex);

            _tradeCapture.LoginResultEvent += (c, d) => SfxTaskAdapter.SetCompleted(d);
            _tradeCapture.LoginErrorEvent += (c, d, ex) => SfxTaskAdapter.SetFailed(d, ex);

            _tradeCapture.LogoutResultEvent += (c, d, i) => SfxTaskAdapter.SetCompleted(d, i);
            _tradeCapture.LogoutErrorEvent += (c, d, ex) => SfxTaskAdapter.SetFailed<LogoutInfo>(d, ex);

            _tradeCapture.DisconnectResultEvent += (c, d, t) => SfxTaskAdapter.SetCompleted(d, t);

            _tradeCapture.SubscribeTradesResultEvent += (c, d, r) => { }; // Required for SubscribeTradesResultEndEvent to work(should be fixed after 2.24.66). Just ignore trade reports we wil request them later with another request
            _tradeCapture.SubscribeTradesResultEndEvent += (c, d) => SfxTaskAdapter.SetCompleted(d);
            _tradeCapture.SubscribeTradesErrorEvent += (c, d, ex) => SfxTaskAdapter.SetFailed(d, ex);

            _tradeCapture.DownloadTradesResultEvent += (c, d, r) => ((BlockingChannel<TradeReportEntity>)d).Write(SfxInterop.Convert(r));
            _tradeCapture.DownloadTradesResultEndEvent += (c, d) => ((BlockingChannel<TradeReportEntity>)d).Close();
            _tradeCapture.DownloadTradesErrorEvent += (c, d, ex) => ((BlockingChannel<TradeReportEntity>)d).Close(ex);
        }


        public Task Deinit()
        {
            return Task.Factory.StartNew(() => _tradeCapture.Dispose());
        }


        public Task ConnectAsync(string address)
        {
            var taskSrc = new TaskCompletionSource<object>();
            _tradeCapture.ConnectAsync(taskSrc, address);
            return taskSrc.Task;
        }

        public Task LoginAsync(string username, string password, string deviceId, string appId, string sessionId)
        {
            var taskSrc = new TaskCompletionSource<object>();
            _tradeCapture.LoginAsync(taskSrc, username, password, deviceId, appId, sessionId);
            return taskSrc.Task;
        }

        public Task LogoutAsync(string message)
        {
            var taskSrc = new TaskCompletionSource<LogoutInfo>();
            _tradeCapture.LogoutAsync(taskSrc, message);
            return taskSrc.Task;
        }

        public Task DisconnectAsync(string text)
        {
            var taskSrc = new TaskCompletionSource<string>();
            _tradeCapture.DisconnectAsync(taskSrc, text);
            return taskSrc.Task;
        }

        public Task SubscribeTradesAsync(bool skipCancel)
        {
            var taskSrc = new TaskCompletionSource<object>();
            _tradeCapture.SubscribeTradesAsync(taskSrc, DateTime.UtcNow.AddMinutes(5), skipCancel); // Request 0 trade reports, we will download them later with separate request
            return taskSrc.WithTimeout();
        }

        public void DownloadTradesAsync(TimeDirection timeDirection, DateTime? from, DateTime? to, bool skipCancel,
            BlockingChannel<TradeReportEntity> stream)
        {
            _tradeCapture.DownloadTradesAsync(stream, timeDirection, from, to, skipCancel);
        }
    }
}
