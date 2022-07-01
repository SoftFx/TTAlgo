using Grpc.Core;
using Grpc.Core.Interceptors;
using System;

namespace TickTrader.Algo.Server.PublicAPI
{
    public class CallOptionsInterceptor : Interceptor
    {
        private const int DefaultRequestTimeout = 10;


        public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(TRequest request, ClientInterceptorContext<TRequest, TResponse> context, AsyncUnaryCallContinuation<TRequest, TResponse> continuation)
        {
            if (context.Options.Deadline != null)
            {
                context = new ClientInterceptorContext<TRequest, TResponse>(context.Method, context.Host,
                    context.Options.WithDeadline(DateTime.UtcNow.AddSeconds(DefaultRequestTimeout)));
            }
            return base.AsyncUnaryCall(request, context, continuation);
        }
    }
}
