using Google.Protobuf;
using Grpc.Core;
using Grpc.Core.Interceptors;
using NLog;
using System;
using System.Threading;
using System.Threading.Tasks;
using TickTrader.Algo.Server.Common;

namespace TickTrader.Algo.Server.PublicAPI
{
    public class ClientLoggerInterceptor : Interceptor
    {
        private ILogger _logger;
        private ApiMessageFormatter _messageFormatter;
        private bool _logMessages;


        public ClientLoggerInterceptor(ILogger logger, ApiMessageFormatter messageFormatter, bool logMessages)
        {
            _logger = logger;
            _messageFormatter = messageFormatter;
            _logMessages = logMessages;
        }


        public override TResponse BlockingUnaryCall<TRequest, TResponse>(TRequest request, ClientInterceptorContext<TRequest, TResponse> context, BlockingUnaryCallContinuation<TRequest, TResponse> continuation)
        {
            throw new UnsupportedException("Blocking calls should not be used");
        }

        public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(TRequest request, ClientInterceptorContext<TRequest, TResponse> context, AsyncUnaryCallContinuation<TRequest, TResponse> continuation)
        {
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
            try
            {
                LogOutMsg(request);
                var response = await asyncAction;
                LogInMsg(response);
                return response;
            }
            catch (UnauthorizedException uex)
            {
                LogMethodError(methodName, request, uex, "Bad access token for");
                throw;
            }
            catch (RpcException rex)
            {
                if (rex.StatusCode == StatusCode.DeadlineExceeded)
                {
                    LogMethodError(methodName, request, "Request timed out");
                    throw new Common.TimeoutException($"Request {nameof(TRequest)} timed out");
                }
                else if (rex.StatusCode == StatusCode.Unknown && rex.Status.Detail == "Stream removed")
                {
                    LogMethodError(methodName, request, "Disconnected while executing");
                    throw new AlgoServerException("Connection error");
                }
                LogMethodError(methodName, request, rex);
                throw;
            }
            catch (Exception ex)
            {
                LogMethodError(methodName, request, ex);
                throw;
            }
        }

        private void LogMethodError<TRequest>(string methodName, TRequest request, string errorMsgPrefix)
        {
            _logger.Error($"{errorMsgPrefix} {methodName}({_messageFormatter.ToJson(request as IMessage)})");
        }

        private void LogMethodError<TRequest>(string methodName, TRequest request, Exception ex, string errorMsgPrefix = "Failed to execute")
        {
            _logger.Error(ex, $"{errorMsgPrefix} {methodName}({_messageFormatter.ToJson(request as IMessage)})");
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
            private ClientLoggerInterceptor _parent;
            private IAsyncStreamReader<T> _innerStream;


            public T Current => _innerStream.Current;


            public GrpcReaderStreamWrapper(ClientLoggerInterceptor parent, IAsyncStreamReader<T> innerStream)
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
            private ClientLoggerInterceptor _parent;
            private IClientStreamWriter<T> _innerStream;


            public WriteOptions WriteOptions
            {
                get => _innerStream.WriteOptions;
                set => _innerStream.WriteOptions = value;
            }


            public GrpcWriterStreamWrapper(ClientLoggerInterceptor parent, IClientStreamWriter<T> innerStream)
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
