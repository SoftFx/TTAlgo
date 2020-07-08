using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using System.Threading.Tasks;
using System.Threading;
using System;
using Google.Protobuf;

namespace TickTrader.Algo.Rpc.OverGrpc
{
    public class GrpcSession : ITransportProxy
    {
        private readonly IAsyncStreamReader<MessagePage> _reader;
        private readonly IAsyncStreamWriter<MessagePage> _writer;
        private readonly AutoResetEvent _resetEvent;
        private readonly TaskCompletionSource<bool> _taskSrc;

        private IObserver<RpcMessage> _msgListener;
        private Task _listenTask;


        public Task<bool> Completion => _taskSrc.Task;


        public GrpcSession(IAsyncStreamReader<MessagePage> reader, IAsyncStreamWriter<MessagePage> writer)
        {
            _reader = reader;
            _writer = writer;

            _resetEvent = new AutoResetEvent(true);
            _taskSrc = new TaskCompletionSource<bool>();
        }


        public void AttachListener(IObserver<RpcMessage> msgListener)
        {
            if (_msgListener != null)
                throw RpcStateException.AnotherTransportListener();

            _msgListener = msgListener;
            _listenTask = ListenRequests();
        }

        public async Task SendResponse(MessagePage response)
        {
            _resetEvent.WaitOne();
            await _writer.WriteAsync(response).ConfigureAwait(false);
            _resetEvent.Set();
        }

        public async void SendMessage(RpcMessage message)
        {
            var page = new MessagePage();
            page.Messages.Add(message.ToByteString());
            await SendResponse(page);
        }

        public async Task Close()
        {
            _taskSrc.TrySetResult(true);
            if (_listenTask != null)
                await _listenTask;
        }


        private async Task ListenRequests()
        {
            try
            {
                while (!Completion.IsCompleted)
                {
                    var hasElement = await _reader.MoveNext().ConfigureAwait(false);
                    if (hasElement)
                    {
                        foreach (var msg in _reader.Current.Messages)
                        {
                            var m = RpcMessage.Parser.ParseFrom(msg);
                            _msgListener.OnNext(m);
                        }
                    }
                    else
                    {
                        _taskSrc.TrySetResult(false);
                        _msgListener.OnCompleted();
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                _taskSrc.TrySetResult(false);
                _msgListener.OnError(ex);
            }
        }
    }
}
