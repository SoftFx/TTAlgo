using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.WellKnownTypes;
using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace TickTrader.Algo.Rpc
{
    public static class RpcHandler
    {
        public static readonly Any VoidResponse = Any.Pack(new VoidResponse());


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGetError(this Any payload, out Exception ex)
        {
            ex = null;
            if (payload.Is(ErrorResponse.Descriptor))
            {
                var error = payload.Unpack<ErrorResponse>();
                ex = new Exception(error.Message);
                return true;
            }
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool SingleReponseHandler<T>(TaskCompletionSource<T> taskSrc, Any payload) where T : IMessage, new()
        {
            if (payload.TryGetError(out var ex))
            {
                taskSrc.TrySetException(ex);
            }
            else
            {
                var response = payload.Unpack<T>();
                taskSrc.TrySetResult(response);
            }

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ListReponseHandler<T>(IObserver<RepeatedField<T>> observer, Any payload) where T : IMessage, new()
        {
            if (TryGetError(payload, out var ex))
            {
                observer.OnError(ex);
            }
            else
            {
                var response = payload.Unpack<T>();
                //observer.OnNext(response.Items);
                //if (!response.IsFinal)
                //    return false;

                observer.OnCompleted();
            }

            return true;
        }
    }
}
