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

        private Channel<UpdateInfo> _channel;
        private ChannelWriter<UpdateInfo> _writer;
        private IServerStreamWriter<UpdateInfo> _networkStream;
        private CancellationTokenSource _closeTokenSrc;
        private Task _dispatchTask;
        private int _closeFlag;


        public SessionInfo Info { get; }

        public string Id { get; }

        public bool IsClosed => _closeFlag != 0;


        public SessionHandler(SessionInfo info)
        {
            Info = info;
            Id = info.Id;
            _logger = info.Logger;
        }


        public bool TryWrite(UpdateInfo info, string logMsg)
        {
            if (_writer == null)
                return true;

            var res = _writer.TryWrite(info);
            if (logMsg != null)
                Info.Logger.Info(logMsg);
            return res;
        }

        public void Close(string reason)
        {
            if (Interlocked.CompareExchange(ref _closeFlag, 1, 0) != 0)
                return;

            _writer?.TryComplete();
            _closeTokenSrc?.Cancel();

            _logger.Info($"Closed update stream - {reason}");
        }

        public Task Open(IServerStreamWriter<UpdateInfo> networkStream)
        {
            if (IsClosed)
                throw new Domain.AlgoException("Session already closed");

            if (_channel != null)
                throw new Domain.AlgoException("Session already opened");

            _channel = DefaultChannelFactory.CreateForSingleConsumer<UpdateInfo>();
            _writer = _channel.Writer;

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
                Close("Dispatch failed");
            }
        }
    }
}
