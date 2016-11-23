using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.Core
{
    [Serializable]
    public abstract class InvokeStartegy
    {
        private Action<ExecutorException> onCoreError;
        private Action<Exception> onRuntimeError;
        private Action<RateUpdate> onFeedUpdate;
        private Action<RateUpdate> onRateUpdate;

        internal InvokeStartegy()
        {
        }

        public void Init(PluginBuilder builder, Action<ExecutorException> onCoreError, Action<Exception> onRuntimeError, Action<RateUpdate> onFeedUpdate)
        {
            this.Builder = builder;
            this.onCoreError = onCoreError;
            this.onRuntimeError = onRuntimeError;
            this.onFeedUpdate = onFeedUpdate;
            OnInit();
        }

        protected PluginBuilder Builder { get; private set; }

        public abstract int FeedQueueSize { get; }

        public abstract void Start();
        public abstract Task Stop(Action<PluginBuilder> finalAction);
        public abstract void Enqueue(QuoteEntity update);
        public abstract void EnqueueCustomAction(Action<PluginBuilder> a);
        public abstract void Enqueue(Action<PluginBuilder> a);

        protected virtual void OnInit() { }

        protected void OnFeedUpdate(RateUpdate update)
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
        private FeedQueue feedQueue;
        private Queue<Action<PluginBuilder>> defaultQueue;
        private bool isStarted;

        protected override void OnInit()
        {
            feedQueue = new FeedQueue();
            defaultQueue = new Queue<Action<PluginBuilder>>();
        }

        public override int FeedQueueSize { get { return 0; } }

        public override void Enqueue(QuoteEntity update)
        {
            lock (syncObj)
            {
                feedQueue.Enqueue(update);
                WakeUpWorker();
            }
        }

        public override void Enqueue(Action<PluginBuilder> a)
        {
            lock (syncObj)
            {
                defaultQueue.Enqueue(a);
                WakeUpWorker();
            }
        }

        public override void EnqueueCustomAction(Action<PluginBuilder> a)
        {
            lock (syncObj)
            {
                defaultQueue.Enqueue(a);
                WakeUpWorker();
            }
        }

        private void WakeUpWorker()
        {
            if (!isStarted)
                return;

            if (currentTask != null)
                return;

            currentTask = Task.Factory.StartNew(ProcessLoop);
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
                    if (item is RateUpdate)
                        OnFeedUpdate((RateUpdate)item);
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
            lock (syncObj)
            {
                isStarted = true;
                WakeUpWorker();
            }
        }

        public override async Task Stop(Action<PluginBuilder> finalAction)
        {
            Task toWait;

            lock (syncObj)
            {
                feedQueue.Clear();
                defaultQueue.Clear();
                defaultQueue.Enqueue(finalAction);
                WakeUpWorker();
                isStarted = false;
                toWait = currentTask;
            }

            if (toWait != null)
                await toWait.ConfigureAwait(false);
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
