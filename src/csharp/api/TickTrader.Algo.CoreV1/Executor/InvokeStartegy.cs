using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TickTrader.Algo.Calculator.AlgoMarket;
using TickTrader.Algo.Core;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.CoreV1
{
    public abstract class InvokeStartegy
    {
        private Action<Exception> _onRuntimeError;

        internal InvokeStartegy()
        {
        }

        internal void Init(IFixtureContext context, Action<Exception> onRuntimeError, FeedStrategy fStrategy)
        {
            Builder = context.Builder;
            MarketData = context.MarketData;
            _onRuntimeError = onRuntimeError;
            FStartegy = fStrategy;
            OnInit();
        }

        public virtual void Reinit()
        {
            OnReinit();
        }

        protected PluginBuilder Builder { get; private set; }
        protected FeedStrategy FStartegy { get; private set; }
        protected AlgoMarketState MarketData { get; private set; }

        public abstract int FeedQueueSize { get; }

        public abstract void Start();
        public abstract Task Stop(bool quick);
        public abstract void Abort();
        public abstract void EnqueueBar(BarRateUpdate update);
        public abstract void EnqueueCustomInvoke(Action<PluginBuilder> a);
        public abstract void EnqueueTradeUpdate(Action<PluginBuilder> a);
        public abstract void EnqueueEvent(IAccountApiEvent e);
        public abstract void ProcessNextTrade();

        protected virtual void OnInit() { }

        protected virtual void OnReinit() { }

        internal BufferUpdateResult OnFeedUpdate(IRateInfo update)
        {
            return FStartegy.ApplyUpdate(update);
        }

        protected void OnRuntimeException(Exception ex)
        {
            _onRuntimeError?.Invoke(ex);
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
        private Queue<IAccountApiEvent> _accEventQueue;
        private bool isStarted;
        private bool isProcessingTrades;
        private bool execStopFlag;
        private Thread currentThread;
        private TaskCompletionSource<object> asyncStopDoneEvent;
        private TaskCompletionSource<bool> stopDoneEvent;

        protected override void OnInit()
        {
            feedQueue = new FeedQueue(FStartegy);
            tradeQueue = new Queue<Action<PluginBuilder>>();
            eventQueue = new Queue<Action<PluginBuilder>>();
            _accEventQueue = new Queue<IAccountApiEvent>();
        }

        protected override void OnReinit()
        {
            lock (syncObj)
            {
                if (isStarted && currentThread != null && currentTask.Status == TaskStatus.Running && isProcessingTrades)
                    UnlockTradeUpdate();
            }
        }

        public override int FeedQueueSize => 0;

        public override void EnqueueBar(BarRateUpdate update)
        {
            lock (syncObj)
            {
                if (execStopFlag)
                    return;

                feedQueue.Enqueue(update);
                WakeUpWorker();
            }
        }

        public override void EnqueueTradeUpdate(Action<PluginBuilder> a)
        {
            lock (syncObj)
            {
                if (execStopFlag)
                    return;

                tradeQueue.Enqueue(a);

                UnlockTradeUpdate();
            }
        }

        private void UnlockTradeUpdate()
        {
            lock (syncObj)
            {
                if (isProcessingTrades)
                    Monitor.Pulse(syncObj);
                else
                    WakeUpWorker();
            }
        }

        public override void EnqueueCustomInvoke(Action<PluginBuilder> a) // use to execute some actions on plugin thread with high priority
        {
            lock (syncObj)
            {
                if (execStopFlag)
                    return;

                eventQueue.Enqueue(a);
                WakeUpWorker();
            }
        }

        public override void EnqueueEvent(IAccountApiEvent e) // use only to fire events on plugin thread
        {
            lock (syncObj)
            {
                if (execStopFlag)
                    return;

                _accEventQueue.Enqueue(e);
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
            try
            {
                lock (syncObj)
                    currentThread = Thread.CurrentThread;

                while (true)
                {
                    object item = null;

                    lock (syncObj)
                    {
                        item = DequeueNext();
                        if (item == null)
                        {
                            currentTask = null;
                            currentThread = null;
                            break;
                        }
                    }

                    ProcessItem(item);
                }
            }
            catch (ThreadAbortException)
            {
                lock (syncObj)
                {
                    currentTask = null;
                    currentThread = null;
                }
                System.Diagnostics.Debug.WriteLine("Process Loop was aborted!");
                Thread.ResetAbort();
            }
        }

        private void ProcessItem(object item)
        {
            try
            {
                if (item is IRateInfo rateInfo)
                    OnFeedUpdate(rateInfo);
                else if (item is Action<PluginBuilder> action)
                    action(Builder);
                else if (item is IAccountApiEvent accEvent)
                    accEvent.Fire(Builder);
            }
            catch (ThreadAbortException) { }
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

                if (isStarted)
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

        public override void Abort()
        {
            lock (syncObj)
            {
                if (execStopFlag || asyncStopDoneEvent != null || stopDoneEvent != null)
                {
                    execStopFlag = true;
                    if (asyncStopDoneEvent != null)
                        Task.Factory.StartNew(() => asyncStopDoneEvent?.TrySetResult(this)); // without this async stop will continue execution on current thread. Which makes DoStop(bool) finish before continue this method
                    if (stopDoneEvent != null)
                        Task.Factory.StartNew(() => stopDoneEvent?.TrySetResult(false));
                    //currentThread?.Abort(); // not supported on net core
                    currentThread = null;
                    Builder.Logger.OnAbort();
                }
            }
        }

        private async Task DoStop(bool quick)
        {
            if (!quick)
            {
                asyncStopDoneEvent = new TaskCompletionSource<object>();
                EnqueueCustomInvoke(async b =>
                {
                    System.Diagnostics.Debug.WriteLine("STRATEGY ASYNC STOP!");
                    try
                    {
                        await b.InvokeAsyncStop();
                    }
                    finally
                    {
                        asyncStopDoneEvent?.TrySetResult(this);
                    }
                    System.Diagnostics.Debug.WriteLine("STRATEGY ASYNC STOP DONE!");
                });

                await asyncStopDoneEvent.Task.ConfigureAwait(false); // wait async stop to end
                asyncStopDoneEvent = null;
            }

            bool aborted;
            lock (syncObj)
            {
                aborted = execStopFlag;
            }
            if (!aborted)
            {
                Task toWait = null;
                stopDoneEvent = new TaskCompletionSource<bool>();
                EnqueueCustomInvoke(b =>
                {
                    try
                    {
                        System.Diagnostics.Debug.WriteLine("STRATEGY CALL OnStop()!");
                        b.InvokeOnStop();
                    }
                    finally
                    {
                        lock (syncObj)
                        {
                            ClearQueues();
                            execStopFlag = true; //  stop queue
                            toWait = currentTask;
                        }
                        stopDoneEvent?.TrySetResult(true);
                    }
                });

                await stopDoneEvent.Task.ConfigureAwait(false);
                stopDoneEvent = null;

                System.Diagnostics.Debug.WriteLine("STRATEGY JOIN!");
                if (toWait != null)
                {
                    try
                    {
                        await toWait.ConfigureAwait(false); // wait current invoke to end
                    }
                    catch { } //we logging this case on ProcessLoop
                }
                System.Diagnostics.Debug.WriteLine("STRATEGY DONE JOIN!");
            }

            lock (syncObj)
            {
                ClearQueues();
                execStopFlag = false;
                isStarted = false;
                stopTask = null;

                System.Diagnostics.Debug.WriteLine("STRATEGY STOP COMPLETED!");
            }
        }

        private void ClearQueues()
        {
            eventQueue.Clear();
            _accEventQueue.Clear();
            tradeQueue.Clear();
            feedQueue.Clear();
        }

        private object DequeueNext()
        {
            if (eventQueue.Count > 0)
                return eventQueue.Dequeue();
            else if (_accEventQueue.Count > 0)
                return _accEventQueue.Dequeue();
            else if (tradeQueue.Count > 0)
                return tradeQueue.Dequeue();
            else if (feedQueue.Count > 0)
                return feedQueue.Dequeue();
            else
                return null;
        }
    }
}
