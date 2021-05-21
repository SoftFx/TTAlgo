using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ActorSharp
{
    public class Actor
    {
        public static event Action<Exception> UnhandledException;

        public static Ref<T> SpawnLocal<T>(IContextFactory factory = null, string actorName = null)
            where T : class, new()
        {
            var instance = new T();
            var context = factory?.CreateContext() ?? new PoolContext(10, actorName);
            var basicActor = instance as Actor;
            if (basicActor != null)
            {
                basicActor.Name = actorName;
                basicActor.Start(context);
            }
            return new LocalRef<T>(instance, context);
        }

        public static Ref<T> SpawnLocal<T>(T instance, IContextFactory factory = null, string actorName = null)
        {
            var basicActor = instance as Actor;
            var context = factory?.CreateContext() ?? new PoolContext(10, actorName);

            if (basicActor != null)
            {
                if (basicActor.Context != null)
                    throw new InvalidOperationException("Provided actor instance is already initialized!");

                basicActor.Name = actorName;
                basicActor.Start(context);
            }

            return new LocalRef<T>(instance, context);
        }

        public static Task Spawn(Func<Task> asyncFunc)
        {
            return Spawn(null, asyncFunc);
        }

        public static Task Spawn(IContextFactory factory, Func<Task> asyncFunc)
        {
            var context = factory?.CreateContext() ?? new PoolContext(10, null);

            var actor = new SingleFunctionActor(asyncFunc);
            context.Post(s => ((SingleFunctionActor)s).Start(), actor);

            return actor.Task;
        }

        public string Name { get; private set; }

        internal static void OnActorFailed(Exception fault)
        {
            UnhandledException?.Invoke(fault);
        }

        internal SynchronizationContext Context { get; set; }
        internal virtual bool PostInit => true;

        private void Start(SynchronizationContext context)
        {
            Context = context ?? throw new Exception("Synchronization context is required!");
            if (PostInit)
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

        #region Non-Actor API

        //private void InvokeContextCheck()
        //{
        //    #if DEBUG
        //    if (SynchronizationContext.Current != null)
        //        throw new InvalidOperationException("It's forbidden to call ContextInvoke() under an actor context. ContextInvoke() can only be called from non-actor thread!");
        //    #endif
        //}

        protected void ContextSend(Action action)
        {
            Context.Post(ExecAction, action);
        }

        /// <summary>
        /// Invokes a delegate under actor's synchronization context.
        /// Warning! This is a potentially blocking operation! Do not call it under an actor context!
        /// </summary>
        protected void ContextInvoke(Action action)
        {
            if (SynchronizationContext.Current != Context)
                ContextInvokeAsync(action).Wait();
            else
                action();
        }

        /// <summary>
        /// Invokes a delegate under actor's synchronization context.
        /// Warning! This is a potentially blocking operation! Do not call it under an actor context!
        /// </summary>
        protected void ContextInvoke(Action<object> action, object state)
        {
            if (SynchronizationContext.Current != Context)
                ContextInvokeAsync(action, state).Wait();
            else
                action(state);
        }

        private Task ContextInvokeAsync(Action action)
        {
            var task = new Task(action);
            Context.Post(ExecTaskSync, task);
            return task;
        }

        private Task ContextInvokeAsync(Action<object> action, object state)
        {
            var task = new Task(action, state);
            Context.Post(ExecTaskSync, task);
            return task;
        }

        protected BlockingChannel<T> CreateBlockingChannel<T>(Channel<T> channel)
        {
            ContextCheck();
            return new BlockingChannel<T>(channel);
        }

        #endregion

        private void ExecAction(object state)
        {
            ((Action)state).Invoke();
        }

        private void ExecTaskSync(object task)
        {
            ((Task)task).RunSynchronously();
        }
    }
}
