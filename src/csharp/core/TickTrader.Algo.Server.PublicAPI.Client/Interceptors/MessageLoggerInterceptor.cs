using Google.Protobuf;
using Grpc.Core;
using Grpc.Core.Interceptors;
using NLog;
using System.Threading;
using System.Threading.Tasks;

namespace TickTrader.Algo.Server.PublicAPI
{
    public class MessageLoggerInterceptor : Interceptor
    {
        private ILogger _logger;
        private ApiMessageFormatter _messageFormatter;
        private bool _logMessages;


        public MessageLoggerInterceptor(ILogger logger, ApiMessageFormatter messageFormatter, bool logMessages)
        {
            _logger = logger;
            _messageFormatter = messageFormatter;
            _logMessages = logMessages;
        }


        public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(TRequest request, ClientInterceptorContext<TRequest, TResponse> context, AsyncUnaryCallContinuation<TRequest, TResponse> continuation)
        {
            LogOutMsg(request);
            var call = continuation(request, context);

            return new AsyncUnaryCall<TResponse>(LogResponseAsync(call.ResponseAsync, context.Method.Name, request),
                call.ResponseHeadersAsync, call.GetStatus, call.GetTrailers, call.Dispose);
        }

        public override AsyncServerStreamingCall<TResponse> AsyncServerStreamingCall<TRequest, TResponse>(TRequest request, ClientInterceptorContext<TRequest, TResponse> context, AsyncServerStreamingCallContinuation<TRequest, TResponse> continuation)
        {
            LogOutMsg(request);
            var call = continuation(request, context);

            return new AsyncServerStreamingCall<TResponse>(new GrpcReaderStreamWrapper<TResponse>(this, call.ResponseStream),
                call.ResponseHeadersAsync, call.GetStatus, call.GetTrailers, call.Dispose);
        }

        public override AsyncClientStreamingCall<TRequest, TResponse> AsyncClientStreamingCall<TRequest, TResponse>(ClientInterceptorContext<TRequest, TResponse> context, AsyncClientStreamingCallContinuation<TRequest, TResponse> continuation)
        {
            var call = continuation(context);

            return new AsyncClientStreamingCall<TRequest, TResponse>(new GrpcWriterStreamWrapper<TRequest>(this, call.RequestStream),
                LogResponseAsync(call.ResponseAsync, context.Method.Name, default(object)),
                call.ResponseHeadersAsync, call.GetStatus, call.GetTrailers, call.Dispose);
        }


        private async Task<TResponse> LogResponseAsync<TRequest, TResponse>(Task<TResponse> asyncAction, string methodName, TRequest request)
        {
            var response = await asyncAction;
            LogInMsg(response);
            return response;
        }


        private void LogOutMsg<TRequest>(TRequest request)
        {
            if (_logMessages && request != null)
            {
                var msg = request as IMessage;
                _messageFormatter.LogMsgToServer(_logger, msg);
            }
        }

        private void LogInMsg<TResponse>(TResponse response)
        {
            if (_logMessages)
            {
                var msg = response as IMessage;
                _messageFormatter.LogMsgFromServer(_logger, msg);
            }
        }


        private class GrpcReaderStreamWrapper<T> : IAsyncStreamReader<T>
        {
            private MessageLoggerInterceptor _parent;
            private IAsyncStreamReader<T> _innerStream;


            public T Current => _innerStream.Current;


            public GrpcReaderStreamWrapper(MessageLoggerInterceptor parent, IAsyncStreamReader<T> innerStream)
            {
                _parent = parent;
                _innerStream = innerStream;
            }


            public async Task<bool> MoveNext(CancellationToken cancellationToken)
            {
                var res = await _innerStream.MoveNext(cancellationToken);
                if (typeof(T) != typeof(UpdateInfo))
                {
                    _parent.LogInMsg(_innerStream.Current);
                }
                return res;
            }
        }

        private class GrpcWriterStreamWrapper<T> : IClientStreamWriter<T>
        {
            private MessageLoggerInterceptor _parent;
            private IClientStreamWriter<T> _innerStream;


            public WriteOptions WriteOptions
            {
                get => _innerStream.WriteOptions;
                set => _innerStream.WriteOptions = value;
            }


            public GrpcWriterStreamWrapper(MessageLoggerInterceptor parent, IClientStreamWriter<T> innerStream)
            {
                _parent = parent;
                _innerStream = innerStream;
            }


            public async Task WriteAsync(T message)
            {
                if (typeof(T) != typeof(UpdateInfo))
                {
                    _parent.LogOutMsg(message);
                }
                await _innerStream.WriteAsync(message);
            }

            public Task CompleteAsync() => _innerStream.CompleteAsync();
        }
    }
}
