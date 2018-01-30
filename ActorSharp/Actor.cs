using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ActorSharp
{
    public class Actor
    {
        public static Ref<T> SpawnLocal<T>(IContextFactory factory = null)
            where T : Actor, new()
        {
            var actor = new T();
            actor.Start(factory?.CreateContext() ?? new PoolContext());
            return new LocalRef<T>(actor);
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

        private void InvokeContextCheck()
        {
            #if DEBUG
            if (SynchronizationContext.Current != null)
                throw new InvalidOperationException("It's forbidden to call ContextInvoke() under an actor context. ContextInvoke() can only be called from non-actor thread!");
            #endif
        }

        /// <summary>
        /// Invokes a delegate under actor's synchronization context.
        /// Warning! This is a potentially blocking operation! Do not call it under an actor context!
        /// </summary>
        protected void ContextInvoke(Action action)
        {
            ContextInvokeAsync(action).Wait();
        }

        /// <summary>
        /// Invokes a delegate under actor's synchronization context.
        /// Warning! This is a potentially blocking operation! Do not call it under an actor context!
        /// </summary>
        protected void ContextInvoke(Action<object> action, object state)
        {
            ContextInvokeAsync(action, state).Wait();
        }

        private Task ContextInvokeAsync(Action action)
        {
            InvokeContextCheck();
            var task = new Task(action);
            Context.Post(ExecTaskSync, task);
            return task;
        }

        private Task ContextInvokeAsync(Action<object> action, object state)
        {
            InvokeContextCheck();
            var task = new Task(action, state);
            Context.Post(ExecTaskSync, task);
            return task;
        }

        private void ExecTaskSync(object task)
        {
            ((Task)task).RunSynchronously();
        }
    }
}
