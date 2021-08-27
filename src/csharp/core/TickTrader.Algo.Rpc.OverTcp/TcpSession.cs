using ActorSharp;
using Google.Protobuf;
using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace TickTrader.Algo.Rpc.OverTcp
{
    public class TcpSession : ITransportProxy
    {
        private const int MsgLengthPrefixSize = 4;
        private const int WriteBatchSize = 8;

        private readonly Socket _socket;
        private readonly Ref<TcpContext> _tcpContext;
        private readonly Pipe _listenPipe, _sendPipe;
        private readonly byte[] _recieveBuffer;
        private readonly Channel<RpcMessage> _readChannel, _writeChannel;

        private Task _listenTask, _sendTask, _readTask, _writeTask;
        private CancellationTokenSource _cancelTokenSrc;


        public ChannelReader<RpcMessage> ReadChannel { get; }

        public ChannelWriter<RpcMessage> WriteChannel { get; }


        public TcpSession(Socket socket, Ref<TcpContext> context)
        {
            _socket = socket;
            _tcpContext = context;
            var options = new PipeOptions(null, null, null, 4 * 1024 * 1024, 1024 * 1024);
            _listenPipe = new Pipe(options);
            _sendPipe = new Pipe(options);

            _readChannel = Channel.CreateUnbounded<RpcMessage>(new UnboundedChannelOptions { AllowSynchronousContinuations = false, SingleWriter = true });
            _writeChannel = Channel.CreateUnbounded<RpcMessage>(new UnboundedChannelOptions { AllowSynchronousContinuations = false, SingleReader = true });

            ReadChannel = _readChannel.Reader;
            WriteChannel = _writeChannel.Writer;

            _recieveBuffer = new byte[socket.ReceiveBufferSize];
        }

        public async Task Start()
        {
            _cancelTokenSrc = new CancellationTokenSource();
            var cancelToken = _cancelTokenSrc.Token;

            _listenTask = await Task.Factory.StartNew(() => ListenLoop(cancelToken), TaskCreationOptions.PreferFairness);
            _sendTask = await Task.Factory.StartNew(() => SendLoop(cancelToken), TaskCreationOptions.PreferFairness);
            _readTask = await Task.Factory.StartNew(() => ReadLoop(cancelToken), TaskCreationOptions.PreferFairness);
            _writeTask = await Task.Factory.StartNew(() => WriteLoop(cancelToken), TaskCreationOptions.PreferFairness);
        }

        public async Task Stop()
        {
            //_cancelTokenSrc.Cancel();
            _readChannel.Writer.TryComplete();
            _writeChannel.Writer.TryComplete();

            await _writeTask;
            await _sendTask;

            await _tcpContext.Call(_ =>
            {
                //_socket.Close();
                try
                {
                    // socket can already be in invalid state
                    _socket.Disconnect(false);
                    _socket.Dispose();
                }
                catch (Exception) { }
            });


            await _listenTask;
            await _readTask;
        }

        public Task Close()
        {
            return Stop();
        }


        private async Task ReadLoop(CancellationToken cancelToken)
        {
            var pipeReader = _listenPipe.Reader;
            var writer = _readChannel.Writer;

            var msgChannelCompleted = false;
            try
            {
                while (!cancelToken.IsCancellationRequested)
                {
                    var res = await pipeReader.ReadAsync().ConfigureAwait(false);
                    var buffer = res.Buffer;
                    if (res.IsCompleted || res.IsCanceled)
                        break;

                    while (!cancelToken.IsCancellationRequested)
                    {
                        if (buffer.Length < MsgLengthPrefixSize)
                            break;

                        var msgSize = buffer.ReadInt32();
                        if (buffer.Length < msgSize + MsgLengthPrefixSize)
                            break;

                        var msg = RpcMessage.Parser.ParseFrom(buffer.Slice(MsgLengthPrefixSize, msgSize));
                        if (!writer.TryWrite(msg))
                        {
                            var canWrite = await writer.WaitToWriteAsync().ConfigureAwait(false);
                            if (!canWrite)
                            {
                                msgChannelCompleted = true;
                                break;
                            }

                            if (!writer.TryWrite(msg))
                                break;
                        }

                        buffer = buffer.Slice(msgSize + MsgLengthPrefixSize);
                    }

                    if (msgChannelCompleted)
                        break;

                    pipeReader.AdvanceTo(buffer.Start, buffer.End);
                }
            }
            catch (Exception) { }

            pipeReader.Complete();
            writer.TryComplete();
        }

        private async Task WriteLoop(CancellationToken cancelToken)
        {
            var pipeWriter = _sendPipe.Writer;
            var reader = _writeChannel.Reader;

            try
            {
                while (!cancelToken.IsCancellationRequested)
                {
                    var canRead = await reader.WaitToReadAsync().ConfigureAwait(false);
                    if (!canRead)
                        break;

                    for (var cnt = 0; cnt < WriteBatchSize && reader.TryRead(out var msg); cnt++)
                    {
                        var msgSize = msg.CalculateSize();
                        var len = msgSize + MsgLengthPrefixSize;
                        var mem = pipeWriter.GetMemory(len);

                        mem.Span.WriteInt32(msgSize);
                        msg.WriteTo(mem.Span.Slice(MsgLengthPrefixSize, msgSize));

                        pipeWriter.Advance(len);
                    }

                    await pipeWriter.FlushAsync().ConfigureAwait(false);
                }
            }
            catch (Exception) { }

            pipeWriter.Complete();
            _writeChannel.Writer.TryComplete();
        }

        private async Task ListenLoop(CancellationToken cancelToken)
        {
            //Func<byte[], AsyncCallback, object, IAsyncResult> socketBeginReceive = (buffer, callback, state) => _socket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, callback, state);
            //Func<IAsyncResult, int> socketEndReceive = _socket.EndReceive;

            var pipeWriter = _listenPipe.Writer;
            try
            {
                while (!cancelToken.IsCancellationRequested)
                {
                    var read = await _socket.ReceiveAsync(new ArraySegment<byte>(_recieveBuffer, 0, _socket.ReceiveBufferSize), SocketFlags.None).ConfigureAwait(false);
                    //var read = await Task.Factory.FromAsync(socketBeginReceive, socketEndReceive, _recieveBuffer, null).ConfigureAwait(false);
                    if (read == 0)
                        break;

                    var buffer = pipeWriter.GetMemory(read);
                    _recieveBuffer.AsSpan(0, read).CopyTo(buffer.Span);
                    pipeWriter.Advance(read);
                    await pipeWriter.FlushAsync().ConfigureAwait(false);
                }
            }
            catch (Exception) { }

            pipeWriter.Complete();
        }

        private async Task SendLoop(CancellationToken cancelToken)
        {
            //Func<byte[], int, AsyncCallback, object, IAsyncResult> socketBeginSend = (buffer, len, callback, state) => _socket.BeginSend(buffer, 0, len, SocketFlags.None, callback, state);
            //Func<IAsyncResult, int> socketEndSend = _socket.EndSend;

            var pipeReader = _sendPipe.Reader;
            try
            {
                while (!cancelToken.IsCancellationRequested)
                {
                    var res = await pipeReader.ReadAsync().ConfigureAwait(false);
                    if (res.IsCanceled || res.IsCompleted)
                        break;

                    foreach (var segment in res.Buffer)
                    {
                        var len = segment.Length;
                        var buffer = ArrayPool<byte>.Shared.Rent(len);
                        try
                        {
                            segment.CopyTo(buffer);
                            await _socket.SendAsync(new ArraySegment<byte>(buffer, 0, len), SocketFlags.None).ConfigureAwait(false);
                            //await Task.Factory.FromAsync(socketBeginSend, socketEndSend, buffer, len, null).ConfigureAwait(false);
                        }
                        finally
                        {
                            ArrayPool<byte>.Shared.Return(buffer);
                        }
                    }
                    pipeReader.AdvanceTo(res.Buffer.End);
                }
            }
            catch (Exception) { }

            pipeReader.Complete();
        }

        #region NetCore draft

        //public async Task ListenLoop(CancellationToken cancelToken)
        //{
        //    await Task.Delay(100).ConfigureAwait(false);

        //    var pipeWriter = _listenPipe.Writer;
        //    while (!cancelToken.IsCancellationRequested)
        //    {
        //        var buffer = pipeWriter.GetMemory(socket.ReceiveBufferSize);
        //        int read = 0;
        //        try
        //        {
        //            read = await socket.ReceiveAsync(buffer, SocketFlags.None, cancelToken).ConfigureAwait(false);
        //        }
        //        catch (OperationCanceledException)
        //        {
        //            pipeWriter.Complete();
        //            return;
        //        }
        //        if (read == 0)
        //        {
        //            pipeWriter.Complete();
        //            return;
        //        }
        //        pipeWriter.Advance(read);
        //        await pipeWriter.FlushAsync().ConfigureAwait(false);
        //    }
        //}

        //private async Task SendLoop(CancellationToken cancelToken)
        //{
        //    await Task.Delay(100).ConfigureAwait(false);

        //    var reader = _sendPipe.Reader;
        //    while (!cancelToken.IsCancellationRequested)
        //    {
        //        var res = await reader.ReadAsync().ConfigureAwait(false);
        //        if (res.IsCanceled || res.IsCompleted)
        //        {
        //            return;
        //        }
        //        foreach (var segment in res.Buffer)
        //        {
        //            await _socket.SendAsync(segment, SocketFlags.None).ConfigureAwait(false);
        //        }
        //        reader.AdvanceTo(res.Buffer.End);
        //    }
        //}

        #endregion NetCore draft
    }
}
