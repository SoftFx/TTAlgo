using Grpc.Core;
using NLog;
using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using TickTrader.Algo.Async;
using TickTrader.Algo.Server.PublicAPI;

namespace TickTrader.Algo.ServerControl.Model
{
    internal sealed class SessionHandler
    {
        private readonly ILogger _logger;
        private readonly Channel<UpdateInfo> _channel;
        private readonly ChannelWriter<UpdateInfo> _writer;

        private IServerStreamWriter<UpdateInfo> _networkStream;
        private CancellationTokenSource _closeTokenSrc;
        private Task _dispatchTask;
        private int _closeFlag;


        public SessionInfo Info { get; }


        public SessionHandler(SessionInfo info)
        {
            Info = info;
            _logger = info.Logger;

            _channel = DefaultChannelFactory.CreateForSingleConsumer<UpdateInfo>();
            _writer = _channel.Writer;
        }


        public bool TryWrite(UpdateInfo info) => _writer.TryWrite(info);

        public void Close(string reason)
        {
            if (Interlocked.CompareExchange(ref _closeFlag, 1, 0) != 0)
                return;

            _writer.TryComplete();
            _closeTokenSrc?.Cancel();

            _logger.Info($"Closed update stream - {reason}");
        }

        public Task Open(IServerStreamWriter<UpdateInfo> networkStream)
        {
            if (_closeTokenSrc != null)
                throw new Domain.AlgoException("Session already opened");

            _closeTokenSrc = new CancellationTokenSource();
            _networkStream = networkStream;

            _dispatchTask = Task.Run(() => _channel.Consume(DispatchUpdate, 16, 1, _closeTokenSrc.Token)); // leave current sync context
            return _dispatchTask;
        }


        private async Task DispatchUpdate(UpdateInfo update)
        {
            try
            {
                await _networkStream.WriteAsync(update);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Failed to dispatch update: Type == {update.Type}");
            }
        }
    }
}
