using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ActorSharp
{
    public class Actor : IActorRef
    {
        public static IActorRef SpawnLocal<T>(SynchronizationContext context = null)
            where T : Actor, new()
        {
            var actor = new T();
            actor.Start(context ?? new PoolContext());
            return actor;
        }

        internal SynchronizationContext Context { get; set; }

        private void Start(SynchronizationContext context)
        {
            Context = context ?? throw new Exception("Synchronization context is required!");
            Context.Post(InvokeInit, null);
        }

        private void InvokeInit(object state)
        {
            ActorInit();
        }

        protected virtual void ActorInit()
        {
        }

        public void PostMessage(object message)
        {
            Context.Post(ProcessMessage, message);
        }

        protected virtual void ProcessMessage(object message)
        {
        }

        protected void ContextCheck()
        {
            #if DEBUG
            if (SynchronizationContext.Current != Context)
                throw new Exception("Synchronization violation! You cannot directly access this object from another context!");
            #endif
        }

        #region IActorRef   

        ITxChannel<T> IActorRef.NewTxChannel<T>()
        {
            throw new NotImplementedException();
        }

        IRxChannel<T> IActorRef.NewRxChannel<T>()
        {
            throw new NotImplementedException();
        }

        Task IActorRef.CallActor<TActor>(Action<TActor> method)
        {
            var task = new Task(o => method((TActor)o), this);
            Context.Post(ExecTaskSync, task);
            return task;
        }

        Task<TResult> IActorRef.CallActor<TActor, TResult>(Func<TActor, TResult> method)
        {
            var task = new Task<TResult>(o => method((TActor)o), this);
            Context.Post(ExecTaskSync, task);
            return task;
        }

        Task IActorRef.CallActor<TActor>(Func<TActor, Task> method)
        {
            var src = new TaskCompletionSource<object>();
            var invokeTask = new Task(o => BindCompletion(method((TActor)o), src), this);
            Context.Post(ExecTaskSync, invokeTask);
            return src.Task;
        }

        Task<TResult> IActorRef.CallActor<TActor, TResult>(Func<TActor, Task<TResult>> method)
        {
            var src = new TaskCompletionSource<TResult>();
            var invokeTask = new Task(o => BindCompletion(method((TActor)o), src), this);
            Context.Post(ExecTaskSync, invokeTask);
            return src.Task;
        }

        IRxChannel<T> IActorRef.Marshal<T>(ITxChannel<T> channel, int pageSize)
        {
            var thisSide = new LocalRxChannel<T>();
            var oppositeSide = (LocalTxChannel<T>)channel;
            oppositeSide.Init(thisSide, pageSize);
            thisSide.Init(oppositeSide);
            return thisSide;
        }

        ITxChannel<T> IActorRef.Marshal<T>(IRxChannel<T> channel, int pageSize)
        {
            var thisSide = new LocalTxChannel<T>();
            var oppositeSide = (LocalRxChannel<T>)channel;
            oppositeSide.Init(thisSide);
            thisSide.Init(oppositeSide, pageSize);
            return thisSide;
        }

        #endregion

        private void ExecTaskSync(object task)
        {
            ((Task)task).RunSynchronously();
        }

        private static void BindCompletion(Task task, TaskCompletionSource<object> src)
        {
            task.ContinueWith(t =>
            {
                if (t.IsFaulted)
                    src.SetException(t.Exception);
                else if (t.IsCanceled)
                    src.SetCanceled();
                else //if(t.IsCompleted)
                    src.SetResult(null);
            });
        }

        private static void BindCompletion<T>(Task<T> task, TaskCompletionSource<T> src)
        {
            task.ContinueWith(t =>
            {
                if (t.IsFaulted)
                    src.SetException(t.Exception);
                else if (t.IsCanceled)
                    src.SetCanceled();
                else //if(t.IsCompleted)
                    src.SetResult(t.Result);
            });
        }
    }
}
