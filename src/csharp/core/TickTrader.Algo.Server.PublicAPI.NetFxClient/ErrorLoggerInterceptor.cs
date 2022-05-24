using Google.Protobuf;
using Grpc.Core;
using Grpc.Core.Interceptors;
using NLog;
using System;
using System.Threading.Tasks;
using TickTrader.Algo.Server.Common;

namespace TickTrader.Algo.Server.PublicAPI
{
    public class ErrorLoggerInterceptor : Interceptor
    {
        private ILogger _logger;
        private ApiMessageFormatter _messageFormatter;


        public ErrorLoggerInterceptor(ILogger logger, ApiMessageFormatter messageFormatter)
        {
            _logger = logger;
            _messageFormatter = messageFormatter;
        }


        public override TResponse BlockingUnaryCall<TRequest, TResponse>(TRequest request, ClientInterceptorContext<TRequest, TResponse> context, BlockingUnaryCallContinuation<TRequest, TResponse> continuation)
        {
            _logger.Error($"Blocking call to {context.Method.Name}");
            throw new UnsupportedException("Blocking calls should not be used");
        }

        public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(TRequest request, ClientInterceptorContext<TRequest, TResponse> context, AsyncUnaryCallContinuation<TRequest, TResponse> continuation)
        {
            try
            {
                var call = continuation(request, context);

                return new AsyncUnaryCall<TResponse>(LogResponseErrorAsync(call.ResponseAsync, context.Method.Name, request),
                    call.ResponseHeadersAsync, call.GetStatus, call.GetTrailers, call.Dispose);
            }
            catch (Exception ex)
            {
                LogError(ex, context.Method.Name, request);
                throw;
            }
        }

        public override AsyncServerStreamingCall<TResponse> AsyncServerStreamingCall<TRequest, TResponse>(TRequest request, ClientInterceptorContext<TRequest, TResponse> context, AsyncServerStreamingCallContinuation<TRequest, TResponse> continuation)
        {
            try
            {
                var call = continuation(request, context);

                return call;
            }
            catch(Exception ex)
            {
                LogError(ex, context.Method.Name, request);
                throw;
            }
        }

        public override AsyncClientStreamingCall<TRequest, TResponse> AsyncClientStreamingCall<TRequest, TResponse>(ClientInterceptorContext<TRequest, TResponse> context, AsyncClientStreamingCallContinuation<TRequest, TResponse> continuation)
        {
            try
            {
                var call = continuation(context);

                return new AsyncClientStreamingCall<TRequest, TResponse>(call.RequestStream,
                    LogResponseErrorAsync(call.ResponseAsync, context.Method.Name, default(object)),
                    call.ResponseHeadersAsync, call.GetStatus, call.GetTrailers, call.Dispose);
            }
            catch (Exception ex)
            {
                LogError(ex, context.Method.Name, default(object));
                throw;
            }
        }


        private async Task<TResponse> LogResponseErrorAsync<TRequest, TResponse>(Task<TResponse> asyncAction, string methodName, TRequest request)
        {
            try
            {
                return await asyncAction;
            }
            catch (Exception ex)
            {
                LogError(ex, methodName, request);
                throw;
            }
        }

        private void LogError<TRequest>(Exception ex, string methodName, TRequest request)
        {
            switch (ex)
            {
                case UnauthorizedException uex:
                    LogMethodError(methodName, request, uex, "Bad access token for");
                    break;
                case RpcException rex:
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
                    break;
                default:
                    LogMethodError(methodName, request, ex);
                    break;
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
    }
}
