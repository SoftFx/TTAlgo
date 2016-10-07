using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace TickTrader.Algo.Core
{
    [Serializable]
    public abstract class InvokeStartegy
    {
        private Action<ExecutorException> onCoreError;
        private Action<Exception> onRuntimeError;
        private Action<FeedUpdate> onFeedUpdate;

        internal InvokeStartegy()
        {
        }

        public void Init(PluginBuilder builder, Action<ExecutorException> onCoreError, Action<Exception> onRuntimeError, Action<FeedUpdate> onFeedUpdate)
        {
            this.Builder = builder;
            this.onCoreError = onCoreError;
            this.onRuntimeError = onRuntimeError;
            this.onFeedUpdate = onFeedUpdate;
            OnInit();
        }

        protected PluginBuilder Builder { get; private set; }

        public abstract void Start();
        public abstract Task Stop();
        public abstract void Abort();
        //public abstract void EnqueueInvoke(Action<PluginBuilder> a);
        //public abstract void EnqueueInvoke(Task t);
        public abstract void Enqueue(FeedUpdate update);
        public abstract void EnqueueCustomAction(Action<PluginBuilder> a);
        public abstract void Enqueue(Action<PluginBuilder> a);
        public abstract FeedUpdate[] DequeueAllQuoteUpdates();

        protected virtual void OnInit() { }

        protected void OnFeedUpdate(FeedUpdate update)
        {
            onFeedUpdate?.Invoke(update);
        }

        protected void OnError(ExecutorException ex)
        {
            onCoreError?.Invoke(ex);
        }

        protected void OnRuntimeException(Exception ex)
        {
            onRuntimeError?.Invoke(ex);
        }
    }

    [Serializable]
    public class PriorityInvokeStartegy : InvokeStartegy
    {
        private object syncObj = new object();
        private Task currentTask;
        private Queue<FeedUpdate> feedQueue;
        private Queue<Action<PluginBuilder>> defaultQueue;
        private bool isStarted;

        protected override void OnInit()
        {
            feedQueue = new Queue<FeedUpdate>();
            defaultQueue = new Queue<Action<PluginBuilder>>();
        }

        public override void Enqueue(FeedUpdate update)
        {
            lock (syncObj)
            {
                if (isStarted)
                {
                    feedQueue.Enqueue(update);
                    EnableProcessing();
                }
            }
        }

        public override void Enqueue(Action<PluginBuilder> a)
        {
            lock (syncObj)
            {
                if (isStarted)
                {
                    defaultQueue.Enqueue(a);
                    EnableProcessing();
                }
            }
        }

        public override void EnqueueCustomAction(Action<PluginBuilder> a)
        {
            lock (syncObj)
            {
                if (isStarted)
                {
                    defaultQueue.Enqueue(a);
                    EnableProcessing();
                }
            }
        }

        public override FeedUpdate[] DequeueAllQuoteUpdates()
        {
            lock (syncObj)
            {
                var snapshot = feedQueue.ToArray();
                feedQueue.Clear();
                return snapshot;
            }
        }

        private void EnableProcessing()
        {
            lock (syncObj)
            {
                if (currentTask != null)
                    return;

                currentTask = Task.Factory.StartNew(ProcessLoop);
            }
        }

        private void ProcessLoop()
        {
            while (true)
            {
                object item = null;

                lock (syncObj)
                {
                    item = DequeueNext();
                    if (item == null)
                    {
                        currentTask = null;
                        break;
                    }
                }

                try
                {
                    if (item is FeedUpdate)
                        OnFeedUpdate((FeedUpdate)item);
                    else if (item is Action<PluginBuilder>)
                        ((Action<PluginBuilder>)item)(Builder);
                }
                catch (ExecutorException ex)
                {
                    OnError(ex);
                }
                catch (Exception ex)
                {
                    OnRuntimeException(ex);
                }
                
            }
        }

        public override void Start()
        {
            lock (syncObj) isStarted = true;
        }

        public override void Abort()
        {
            lock (syncObj)
            {
                isStarted = false;
                feedQueue.Clear();
                defaultQueue.Clear();
            }
        }

        public override async Task Stop()
        {
            Task toWait;

            lock (syncObj)
            {
                isStarted = false;
                toWait = currentTask;
            }

            if (toWait != null)
                await toWait;
        }

        private object DequeueNext()
        {
            if (defaultQueue.Count > 0)
                return defaultQueue.Dequeue();
            else if (feedQueue.Count > 0)
                return feedQueue.Dequeue();
            else
                return null;
        }
    }


    //[Serializable]
    //public class DataflowInvokeStartegy : InvokeStartegy
    //{
    //    private ActionBlock<object> taskQueue;
    //    private CancellationTokenSource cancelSrc;

    //    public override void Start()
    //    {
    //        this.cancelSrc = new CancellationTokenSource();
    //        var options = new ExecutionDataflowBlockOptions() { MaxDegreeOfParallelism = 1, CancellationToken = cancelSrc.Token };
    //        taskQueue = new ActionBlock<object>((Action<object>)Process, options);
    //    }

    //    public async override Task Stop()
    //    {
    //        try
    //        {
    //            taskQueue.Complete();
    //            await taskQueue.Completion;
    //        }
    //        catch (OperationCanceledException) { }
    //    }

    //    public override void Abort()
    //    {
    //        cancelSrc.Cancel();
    //    }

    //    public override void EnqueueInvoke(Action<PluginBuilder> a)
    //    {
    //        taskQueue.Post(a);
    //    }

    //    public override void EnqueueInvoke(Task t)
    //    {
    //        taskQueue.Post(t);   
    //    }

    //    private void Enqueue(Action a)
    //    {
    //        taskQueue.Post(a);
    //    }

    //    private void Process(object data)
    //    {
    //        try
    //        {
    //            if (data is Action<PluginBuilder>)
    //                ((Action<PluginBuilder>)data)(Builder);
    //            if (data is Task)
    //                ((Task)data).RunSynchronously();
    //        }
    //        catch (ExecutorException ex)
    //        {
    //            OnError(ex);
    //        }
    //        catch (Exception ex)
    //        {
    //            OnRuntimeException(ex);
    //        }
    //    }
    //}
}
