using Google.Protobuf;
using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace TickTrader.Algo.Rpc.OverTcp
{
    public class TcpSession : ITransportProxy
    {
        private readonly Socket _socket;
        private readonly Pipe _listenPipe, _sendPipe;
        private readonly byte[] _recieveBuffer;

        private Task _listenTask, _sendTask, _readTask;
        private CancellationTokenSource _cancelTokenSrc;
        private IObserver<RpcMessage> _msgListener;


        public TcpSession(Socket socket)
        {
            _socket = socket;
            var options = new PipeOptions(null, null, null, 4 * 1024 * 1024, 1024 * 1024);
            _listenPipe = new Pipe(options);
            _sendPipe = new Pipe(options);

            _recieveBuffer = new byte[socket.ReceiveBufferSize];
        }

        public async Task Start()
        {
            _cancelTokenSrc = new CancellationTokenSource();
            var cancelToken = _cancelTokenSrc.Token;

            _listenTask = await Task.Factory.StartNew(() => ListenLoop(cancelToken));
            _sendTask = await Task.Factory.StartNew(() => SendLoop(cancelToken));
        }

        public async Task Stop()
        {
            _cancelTokenSrc.Cancel();

            await _listenTask;
            await _sendTask;
            if (_readTask != null)
                await _readTask;

            _socket.Close();
        }

        public void AttachListener(IObserver<RpcMessage> msgListener)
        {
            if (_msgListener != null)
                throw RpcStateException.AnotherTransportListener();

            _msgListener = msgListener;
            Task.Factory.StartNew(() => ReadLoop(_cancelTokenSrc.Token))
                .ContinueWith(t => _readTask = t.Result);
        }

        public void SendMessage(RpcMessage msg)
        {
            //var sw = System.Diagnostics.Stopwatch.StartNew();

            var msgSize = msg.CalculateSize();
            var len = msgSize + 4;
            var buffer = ArrayPool<byte>.Shared.Rent(msgSize + 4);
            var codedStream = new CodedOutputStream(buffer);
            codedStream.WriteFixed32((uint)msgSize);
            msg.WriteTo(codedStream);

            var pipeWriter = _sendPipe.Writer;
            var memory = pipeWriter.GetMemory(len);
            buffer.AsSpan(0, len).CopyTo(memory.Span);
            ArrayPool<byte>.Shared.Return(buffer);

            pipeWriter.Advance(len);
            pipeWriter.FlushAsync();
            //pipeWriter.FlushAsync().AsTask().ContinueWith(t =>
            //{
            //    sw.Stop();
            //    System.Diagnostics.Debug.WriteLine($"Tcp.SendMessage: {sw.ElapsedMilliseconds} ms");
            //});
        }

        public Task Close()
        {
            return Stop();
        }

        private async Task ReadLoop(CancellationToken cancelToken)
        {
            const int int32Size = 4;

            var pipeReader = _listenPipe.Reader;
            while (!cancelToken.IsCancellationRequested)
            {
                var res = await pipeReader.ReadAsync().ConfigureAwait(false);
                var buffer = res.Buffer;

                while (true)
                {
                    if (buffer.Length < int32Size)
                        break;

                    var msgSize = buffer.ReadInt32();
                    if (buffer.Length < msgSize + int32Size)
                        break;

                    var bytes = ArrayPool<byte>.Shared.Rent(msgSize);
                    buffer.Slice(int32Size).ReadBytes(bytes, msgSize);
                    var msg = RpcMessage.Parser.ParseFrom(bytes, 0, msgSize);
                    ArrayPool<byte>.Shared.Return(bytes);
                    buffer = buffer.Slice(msgSize + int32Size);

                    _msgListener.OnNext(msg);
                }

                pipeReader.AdvanceTo(buffer.Start, buffer.End);
            }
        }

        private async Task ListenLoop(CancellationToken cancelToken)
        {
            Func<byte[], AsyncCallback, object, IAsyncResult> socketBeginReceive = (buffer, callback, state) => _socket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, callback, state);
            Func<IAsyncResult, int> socketEndReceive = _socket.EndReceive;

            var pipeWriter = _listenPipe.Writer;
            while (!cancelToken.IsCancellationRequested)
            {
                try
                {
                    var read = await Task.Factory.FromAsync(socketBeginReceive, socketEndReceive, _recieveBuffer, null).ConfigureAwait(false);
                    if (read == 0)
                    {
                        pipeWriter.Complete();
                        return;
                    }
                    var buffer = pipeWriter.GetMemory(read);
                    _recieveBuffer.AsSpan(0, read).CopyTo(buffer.Span);
                    pipeWriter.Advance(read);
                    await pipeWriter.FlushAsync().ConfigureAwait(false);
                }
                catch (Exception)
                {
                    pipeWriter.Complete();
                    return;
                }
            }
        }

        private async Task SendLoop(CancellationToken cancelToken)
        {
            Func<byte[], int, AsyncCallback, object, IAsyncResult> socketBeginSend = (buffer, len, callback, state) => _socket.BeginSend(buffer, 0, len, SocketFlags.None, callback, state);
            Func<IAsyncResult, int> socketEndSend = _socket.EndSend;

            var reader = _sendPipe.Reader;
            while (!cancelToken.IsCancellationRequested)
            {
                var res = await reader.ReadAsync().ConfigureAwait(false);
                if (res.IsCanceled || res.IsCompleted)
                {
                    return;
                }
                foreach (var segment in res.Buffer)
                {
                    if (cancelToken.IsCancellationRequested)
                        return;

                    var len = segment.Length;
                    var buffer = ArrayPool<byte>.Shared.Rent(len);
                    try
                    {
                        segment.CopyTo(buffer);
                        await Task.Factory.FromAsync(socketBeginSend, socketEndSend, buffer, len, null).ConfigureAwait(false);
                    }
                    finally
                    {
                        ArrayPool<byte>.Shared.Return(buffer);
                    }
                }
                reader.AdvanceTo(res.Buffer.End);
            }
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
