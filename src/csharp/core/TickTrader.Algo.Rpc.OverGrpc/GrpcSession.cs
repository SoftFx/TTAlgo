using Grpc.Core;
using System.Threading.Tasks;
using System;
using Google.Protobuf;
using System.Threading.Channels;

namespace TickTrader.Algo.Rpc.OverGrpc
{
    public class GrpcSession : ITransportProxy
    {
        private const int MessagePageSize = 8;


        private readonly IAsyncStreamReader<MessagePage> _reader;
        private readonly IAsyncStreamWriter<MessagePage> _writer;
        private readonly TaskCompletionSource<bool> _taskSrc;
        private readonly Channel<RpcMessage> _readChannel, _writeChannel;

        private Task _listenTask, _sendTask;


        public Task<bool> Completion => _taskSrc.Task;

        public ChannelReader<RpcMessage> ReadChannel { get; }

        public ChannelWriter<RpcMessage> WriteChannel { get; }


        public GrpcSession(IAsyncStreamReader<MessagePage> reader, IAsyncStreamWriter<MessagePage> writer)
        {
            _reader = reader;
            _writer = writer;

            _taskSrc = new TaskCompletionSource<bool>();

            _readChannel = System.Threading.Channels.Channel.CreateUnbounded<RpcMessage>(new UnboundedChannelOptions { AllowSynchronousContinuations = false, SingleWriter = true });
            _writeChannel = System.Threading.Channels.Channel.CreateUnbounded<RpcMessage>(new UnboundedChannelOptions { AllowSynchronousContinuations = false, SingleReader = true });

            ReadChannel = _readChannel.Reader;
            WriteChannel = _writeChannel.Writer;

            _listenTask = ListenRequests();
            _sendTask = SendRequests();

        }

        public async Task Close()
        {
            _taskSrc.TrySetResult(true);
            await _listenTask;
            await _sendTask;
        }


        private async Task SendRequests()
        {
            var reader = _writeChannel.Reader;

            try
            {
                while (!Completion.IsCompleted)
                {
                    var canRead = await reader.WaitToReadAsync().ConfigureAwait(false);
                    if (!canRead)
                        break;

                    var page = new MessagePage();
                    for (var cnt = 0; reader.TryRead(out var msg) && cnt < MessagePageSize; cnt++) page.Messages.Add(msg.ToByteString());

                    await _writer.WriteAsync(page).ConfigureAwait(false);
                }
            }
            catch(Exception ex)
            {
                _taskSrc.TrySetResult(false);
                //_msgListener.OnError(ex);
            }
        }

        private async Task ListenRequests()
        {
            var writer = _readChannel.Writer;

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
                            writer.TryWrite(m);
                        }
                    }
                    else
                    {
                        _taskSrc.TrySetResult(false);
                        //_msgListener.OnCompleted();
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                _taskSrc.TrySetResult(false);
                //_msgListener.OnError(ex);
            }
        }
    }
}
