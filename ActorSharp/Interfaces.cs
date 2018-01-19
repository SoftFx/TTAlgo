using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ActorSharp
{
    public interface IActorRef
    {
        void PostMessage(object message);

        ITxChannel<T> NewTxChannel<T>();
        IRxChannel<T> NewRxChannel<T>();

        Task CallActor<TActor>(Action<TActor> method);
        Task CallActor<TActor>(Func<TActor, Task> method);
        Task<TResult> CallActor<TActor, TResult>(Func<TActor, TResult> method);
        Task<TResult> CallActor<TActor, TResult>(Func<TActor, Task<TResult>> method);
        IRxChannel<T> Marshal<T>(ITxChannel<T> channel, int pageSize = 10);
        ITxChannel<T> Marshal<T>(IRxChannel<T> channel, int pageSize = 10);
    }

    public interface ITxChannel<T>
    {
        IAwaitable<bool> TryWrite(T item);
        IAwaitable<bool> Write(T item); // throws exceptions

        IAwaitable Close();
    }

    public interface IRxChannel<T> : IAwaitable<bool>
    {
        T Current { get; }
    }

    public interface IAwaitable<T>
    {
        IAwaiter<T> GetAwaiter();
    }

    public interface IAwaiter<T> : INotifyCompletion
    {
        bool IsCompleted { get; }
        T GetResult();
    }

    public interface IAwaitable
    {
        IAwaiter GetAwaiter();
    }

    public interface IAwaiter : INotifyCompletion
    {
        bool IsCompleted { get; }
        void GetResult();
    }

    public interface IActorFactory
    {
        IActorRef Spawn<T>() where T : Actor, new();
    }
}
