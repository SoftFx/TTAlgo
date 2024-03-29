﻿using System;
using System.Threading;
using System.Threading.Tasks;

namespace TickTrader.Algo.Async.Actors
{
    public class CallbackMsg
    {
        public SendOrPostCallback Callback { get; }

        public object State { get; }


        public CallbackMsg(SendOrPostCallback callback, object state)
        {
            Callback = callback;
            State = state;
        }
    }

    public class AskMsg : TaskCompletionSource<object>, IAskMsg
    {
        public object Request { get; }


        public AskMsg(object request)
        {
            Request = request;
        }


        public void SetResponse(object response)
        {
            if (response is Task tRes)
            {
                tRes.ContinueWith(t =>
                {
                    if (t.IsCanceled)
                        SetCanceled();
                    else if (t.IsFaulted)
                        SetException(t.Exception);
                    else SetResult(null);
                });
            }
            else if (response is Exception ex)
                SetException(ex);
            else SetResult(null);
        }
    }

    public class AskMsg<TResponse> : TaskCompletionSource<TResponse>, IAskMsg
    {
        public object Request { get;  }


        public AskMsg(object request)
        {
            Request = request;
        }


        public void SetResponse(object response)
        {
            if (response is Task<TResponse> tRes)
            {
                tRes.ContinueWith(t =>
                {
                    if (t.IsCanceled)
                        SetCanceled();
                    else if (t.IsFaulted)
                        SetException(t.Exception);
                    else SetResult(tRes.Result);
                });
            }
            else if (response is Exception ex)
                SetException(ex);
            else if (response is TResponse)
                SetResult((TResponse)response);
            else if (response is null)
                SetResult(default(TResponse));
            else SetException(Errors.InvalidResponseType(typeof(TResponse), response.GetType()));
        }
    }
}
