using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Machinarium.ActorModel
{
    public abstract class Actor
    {
        private ActorScope scope;

        public Actor()
        {
            scope = ActorScope.GetScope();
        }

        public Actor(ActorScope scope)
        {
            if (scope == null)
                this.scope = ActorScope.GetScope();
            else
                this.scope = scope;
        }

        protected void Enqueue(Action actorAction)
        {
            if (scope == SynchronizationContext.Current)
            {
                try
                {
                    actorAction();
                }
                catch (Exception ex)
                {
                    Environment.FailFast("Unhandled exception in Actor!", ex);
                }
            }
            else
                scope.Enqueue(actorAction);
        }

        protected Task<TResult> AsyncCall<TResult>(Func<Task<TResult>> asyncFunction)
        {
            TaskCompletionSource<TResult> src = new TaskCompletionSource<TResult>();
            Enqueue(() => asyncFunction().ContinueWith(t =>
            {
                if (t.IsFaulted)
                    src.SetException(t.Exception);
                else if (t.IsCanceled)
                    src.SetCanceled();
                else
                    src.SetResult(t.Result);
            }));
            return src.Task;
        }

        protected Task AsyncCall(Func<Task> asyncFunction)
        {
            TaskCompletionSource<object> src = new TaskCompletionSource<object>();
            Enqueue(() => asyncFunction().ContinueWith(t =>
            {
                if (t.IsFaulted)
                    src.SetException(t.Exception);
                else if (t.IsCanceled)
                    src.SetCanceled();
                else
                    src.SetResult(this);
            }));
            return src.Task;
        }

        protected TxChannel<T> InChannel<T>(Action<TxChannel<T>> channelFunc)
        {
            throw new NotImplementedException();
        }

        protected RxChannel<T> OutChannel<T>(Action<TxChannel<T>> channelFunc)
        {
            throw new NotImplementedException();
        }
    }

    public abstract class ActorScope : SynchronizationContext
    {
        public static ActorScope GetScope()
        {
            var currentActorScope = Current as ActorScope;

            if (currentActorScope != null)
                return currentActorScope;

            return new DataflowScope(ActorContextOptions.OwnContext);
        }

        public ActorScope()
        {
        }

        public abstract void Enqueue(Action actorAction);

        protected void InvokeAction(Action actorAction)
        {
            var oldContext = Current;
            SetSynchronizationContext(this);
            try
            {
                actorAction();
            }
            catch (Exception ex)
            {
                Environment.FailFast("Unhandled exception in Actor!", ex);
            }
            finally
            {
                SetSynchronizationContext(oldContext);
            }
        }

        public override void Post(SendOrPostCallback d, object state)
        {
            Enqueue(() => d(state));
        }

        public override void Send(SendOrPostCallback d, object state)
        {
            throw new NotImplementedException();
        }
    }

    public class DataflowScope : ActorScope
    {   
        private ActionBlock<Action> block;

        public DataflowScope(ActorContextOptions contextOption = ActorContextOptions.OwnContext)
        {
            var options = new ExecutionDataflowBlockOptions();

            if (contextOption == ActorContextOptions.InheritContext)
                options.TaskScheduler = TaskScheduler.FromCurrentSynchronizationContext();

            block = new ActionBlock<Action>(a => InvokeAction(a));
        }

        internal ActionBlock<Action> QueueBlock => block;

        protected SynchronizationContext Context { get; private set; }

        public override void Enqueue(Action actorAction)
        {
            block.Post(actorAction);
        }
    }


    public enum ActorMessageType { Action, AsyncAction, AsyncFunc, ChannelData }

    public enum ActorContextOptions
    {
        OwnContext,
        InheritContext
    }
}
