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

        internal InvokeStartegy()
        {
        }

        public void Init(PluginBuilder builder, Action<ExecutorException> onCoreError, Action<Exception> onRuntimeError, FeedStrategy fStrategy)
        {
            this.Builder = builder;
            this.onCoreError = onCoreError;
            this.onRuntimeError = onRuntimeError;
            FStartegy = fStrategy;
            OnInit();
        }

        protected PluginBuilder Builder { get; private set; }
        protected FeedStrategy FStartegy { get; private set; }

        public abstract int FeedQueueSize { get; }

        public abstract void Start();
        public abstract Task Stop(bool quick);
        public abstract void EnqueueQuote(QuoteEntity update);
        public abstract void EnqueueCustomInvoke(Action<PluginBuilder> a);
        public abstract void EnqueueTradeUpdate(Action<PluginBuilder> a);
        public abstract void EnqueueTradeEvent(Action<PluginBuilder> a);
        public abstract void ProcessNextTrade();

        protected virtual void OnInit() { }

        protected void OnFeedUpdate(RateUpdate update)
        {
            FStartegy.ApplyUpdate(update);
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
        private Task stopTask;
        private FeedQueue feedQueue;
        private Queue<Action<PluginBuilder>> tradeQueue;
        private Queue<Action<PluginBuilder>> eventQueue;
        private bool isStarted;
        private bool isProcessingTrades;
        private bool execStopFlag;

        protected override void OnInit()
        {
            feedQueue = new FeedQueue(FStartegy);
            tradeQueue = new Queue<Action<PluginBuilder>>();
            eventQueue = new Queue<Action<PluginBuilder>>();
        }

        public override int FeedQueueSize { get { return 0; } }

        public override void EnqueueQuote(QuoteEntity update)
        {
            lock (syncObj)
            {
                feedQueue.Enqueue(update);
                WakeUpWorker();
            }
        }

        public override void EnqueueTradeUpdate(Action<PluginBuilder> a)
        {
            lock (syncObj)
            {
                tradeQueue.Enqueue(a);
                if (isProcessingTrades)
                    Monitor.Pulse(syncObj);
                else
                    WakeUpWorker();
            }
        }

        public override void EnqueueCustomInvoke(Action<PluginBuilder> a)
        {
            lock (syncObj)
            {
                eventQueue.Enqueue(a);
                WakeUpWorker();
            }
        }

        public override void EnqueueTradeEvent(Action<PluginBuilder> a)
        {
            lock (syncObj)
            {
                eventQueue.Enqueue(a);
                WakeUpWorker();
            }
        }

        public override void ProcessNextTrade()
        {
            Action<PluginBuilder> action;

            lock (syncObj)
            {
                isProcessingTrades = true;
                action = DequeueNextTrade();
            }

            if (action != null)
                ProcessItem(action);

            lock (syncObj) isProcessingTrades = false;
        }

        private Action<PluginBuilder> DequeueNextTrade()
        {
            if (tradeQueue.Count > 0)
                return tradeQueue.Dequeue();

            Monitor.Wait(syncObj);

            if (tradeQueue.Count > 0)
                return tradeQueue.Dequeue();

            return null;
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
                    if (execStopFlag)
                    {
                        currentTask = null;
                        break;
                    }
                    item = DequeueNext();
                    if (item == null)
                    {
                        currentTask = null;
                        break;
                    }
                }

                ProcessItem(item);
            }
        }

        private void ProcessItem(object item)
        {
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

        public override void Start()
        {
            lock (syncObj)
            {
                System.Diagnostics.Debug.WriteLine("STRATEGY START!");

                if (isStarted )
                    throw new InvalidOperationException("Cannot start: Strategy is already running!");

                isStarted = true;
                execStopFlag = false;
                stopTask = null;
                WakeUpWorker();
            }
        }

        public override Task Stop(bool quick)
        {
            lock (syncObj)
            {
                System.Diagnostics.Debug.WriteLine("STRATEGY STOP! qucik=" + quick);

                if (stopTask == null)
                    stopTask = DoStop(quick);
                return stopTask;
            }
        }

        private async Task DoStop(bool quick)
        {
            if (!quick)
            {
                System.Diagnostics.Debug.WriteLine("STRATEGY ASYNC STOP!");

                TaskCompletionSource<object> asyncStopDoneEvent = new TaskCompletionSource<object>();
                EnqueueTradeUpdate(async b =>
                {
                    await b.InvokeAsyncStop();
                    asyncStopDoneEvent.TrySetResult(this);
                });

                await asyncStopDoneEvent.Task.ConfigureAwait(false); // wait async stop to end

                System.Diagnostics.Debug.WriteLine("STRATEGY ASYNC STOP DONE!");
            }

            Task toWait = null;
            lock (syncObj)
            {
                execStopFlag = true; //  stop queue processing
                toWait = currentTask;
            }

            if (toWait != null)
            {
                System.Diagnostics.Debug.WriteLine("STRATEGY WAIT!");
                await toWait.ConfigureAwait(false); // wait current invoke to end
                System.Diagnostics.Debug.WriteLine("STRATEGY DONE WAIT!");
            }

            if (!quick)
            {
                System.Diagnostics.Debug.WriteLine("STRATEGY CALL OnStop()!");
                Builder.InvokeOnStop(); // Invoke OnStop(). This is last invoke. No more invokes are possible after this point.
            }

            lock (syncObj)
            {
                eventQueue.Clear();
                tradeQueue.Clear();
                feedQueue.Clear();
                isStarted = false;
                stopTask = null;

                System.Diagnostics.Debug.WriteLine("STRATEGY STOP COMPLETED!");
            }
        }

        private object DequeueNext()
        {
            if (eventQueue.Count > 0)
                return eventQueue.Dequeue();
            else if (tradeQueue.Count > 0)
                return tradeQueue.Dequeue();
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
