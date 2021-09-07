using System;
using System.Runtime.CompilerServices;

namespace TickTrader.Algo.Async
{
    public interface IAwaitable<T>
    {
        IAwaiter<T> GetAwaiter();
    }

    public interface IAwaiter<T> : INotifyCompletion
    {
        bool IsCompleted { get; }
        T GetResult();
    }

    internal interface IAsyncTokenOwner
    {
        void Return(ReusableAsyncToken token);
    }

    public interface IAsyncToken : IAwaitable<IDisposable> { }


    internal class ReusableAsyncToken : IDisposable, IAsyncToken, IAwaiter<IDisposable>
    {
        private readonly bool _returnSelfToOwner;

        private bool _isCompleted;
        private Action _callback;
        private IAsyncTokenOwner _owner;


        public string WaiterName { get; set; }


        public ReusableAsyncToken(IAsyncTokenOwner owner)
            : this(owner, true)
        {
            _owner = owner;
        }

        private ReusableAsyncToken(IAsyncTokenOwner owner, bool returnSelfToOwner)
        {
            _owner = owner;
            _returnSelfToOwner = returnSelfToOwner;
        }


        public static ReusableAsyncToken CreateCompleted(IAsyncTokenOwner owner) => new ReusableAsyncToken(owner, false) { _isCompleted = true };


        public void Reset()
        {
            _isCompleted = false;
            _callback = null;
            WaiterName = null;
        }

        public void SetCompleted()
        {
            _isCompleted = true;
            _callback?.Invoke();
        }

        public void Dispose()
        {
            _owner?.Return(_returnSelfToOwner ? this : null);
        }


        bool IAwaiter<IDisposable>.IsCompleted => _isCompleted;

        IAwaiter<IDisposable> IAwaitable<IDisposable>.GetAwaiter()
        {
            return this;
        }

        IDisposable IAwaiter<IDisposable>.GetResult()
        {
            return this;
        }

        void INotifyCompletion.OnCompleted(Action continuation)
        {
            _callback = continuation;
        }
    }
}
