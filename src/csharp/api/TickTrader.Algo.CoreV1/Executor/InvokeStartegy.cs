﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Schedulers;
using TickTrader.Algo.Calculator.AlgoMarket;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.CoreV1
{
    public abstract class InvokeStartegy
    {
        private Action<Exception> _onRuntimeError;

        protected IAlgoLogger _logger;


        internal void Init(IFixtureContext context, Action<Exception> onRuntimeError, FeedStrategy fStrategy, IAlgoLogger logger)
        {
            Builder = context.Builder;
            MarketData = context.MarketData;
            _onRuntimeError = onRuntimeError;
            FStartegy = fStrategy;
            _logger = logger;
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
        public abstract void EnqueueQuote(QuoteInfo quote);
        public abstract void EnqueueBar(BarUpdate update);
        public abstract void EnqueueCustomInvoke(Action<PluginBuilder> a);
        public abstract void EnqueueTradeUpdate(Action<PluginBuilder> a);
        public abstract void EnqueueEvent(IAccountApiEvent e);
        public abstract void ProcessNextTrade();

        protected virtual void OnInit() { }

        protected virtual void OnReinit() { }

        internal BufferUpdateResult OnFeedUpdate(FeedUpdateSummary update)
        {
            return FStartegy.ApplyUpdate(update);
        }

        protected void OnRuntimeException(Exception ex)
        {
            _onRuntimeError?.Invoke(ex);
        }
    }


    public class PriorityInvokeStartegy : InvokeStartegy
    {
        private static readonly LimitedConcurrencyLevelTaskScheduler _indicatorScheduler = new LimitedConcurrencyLevelTaskScheduler(2);

        private static int _activeIndicatorCnt; // protected by lock(_indicatorScheduler)

        private readonly object _syncObj = new object();
        private readonly FeedUpdateSummary _feedUpdate = new FeedUpdateSummary();
        private readonly TaskScheduler _scheduler;
        private readonly Action _workerAction; // cache this obj delegate to avoid Action allocation
        private readonly bool _isIndicator;


        private Task currentTask;
        private Task stopTask;
        private FeedQueue2 feedQueue;
        private Queue<Action<PluginBuilder>> tradeQueue;
        private Queue<Action<PluginBuilder>> eventQueue;
        private Queue<IAccountApiEvent> _accEventQueue;
        private bool isStarted;
        private bool isProcessingTrades;
        private bool execStopFlag;
        private Thread currentThread;
        private TaskCompletionSource<object> asyncStopDoneEvent;
        private TaskCompletionSource<bool> stopDoneEvent;


        public PriorityInvokeStartegy(bool isIndicator)
        {
            _isIndicator = isIndicator;
            _scheduler = _isIndicator ? _indicatorScheduler : TaskScheduler.Default;
            _workerAction = ProcessLoop;
        }


        protected override void OnInit()
        {
            feedQueue = new FeedQueue2();
            tradeQueue = new Queue<Action<PluginBuilder>>();
            eventQueue = new Queue<Action<PluginBuilder>>();
            _accEventQueue = new Queue<IAccountApiEvent>();
        }

        protected override void OnReinit()
        {
            lock (_syncObj)
            {
                if (isStarted && currentThread != null && currentTask.Status == TaskStatus.Running && isProcessingTrades)
                    UnlockTradeUpdate();
            }
        }

        public override int FeedQueueSize => feedQueue.Count;

        public override void EnqueueQuote(QuoteInfo quote)
        {
            lock (_syncObj)
            {
                if (execStopFlag)
                    return;

                feedQueue.Enqueue(quote);
                WakeUpWorker();
            }
        }

        public override void EnqueueBar(BarUpdate update)
        {
            lock (_syncObj)
            {
                if (execStopFlag)
                    return;

                feedQueue.Enqueue(update);
                WakeUpWorker();
            }
        }

        public override void EnqueueTradeUpdate(Action<PluginBuilder> a)
        {
            lock (_syncObj)
            {
                if (execStopFlag)
                    return;

                tradeQueue.Enqueue(a);

                UnlockTradeUpdate();
            }
        }

        private void UnlockTradeUpdate()
        {
            lock (_syncObj)
            {
                if (isProcessingTrades)
                    Monitor.Pulse(_syncObj);
                else
                    WakeUpWorker();
            }
        }

        public override void EnqueueCustomInvoke(Action<PluginBuilder> a) // use to execute some actions on plugin thread with high priority
        {
            lock (_syncObj)
            {
                if (execStopFlag)
                    return;

                eventQueue.Enqueue(a);
                WakeUpWorker();
            }
        }

        public override void EnqueueEvent(IAccountApiEvent e) // use only to fire events on plugin thread
        {
            lock (_syncObj)
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

            lock (_syncObj)
            {
                isProcessingTrades = true;
                action = DequeueNextTrade();
            }

            if (action != null)
                ProcessItem(action);

            lock (_syncObj) isProcessingTrades = false;
        }

        private Action<PluginBuilder> DequeueNextTrade()
        {
            if (tradeQueue.Count > 0)
                return tradeQueue.Dequeue();

            Monitor.Wait(_syncObj);

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

            // _workerAction == ProcessLoop
            currentTask = Task.Factory.StartNew(_workerAction, CancellationToken.None, TaskCreationOptions.None, _scheduler);
        }

        private void ProcessLoop()
        {
            try
            {
                lock (_syncObj)
                    currentThread = Thread.CurrentThread;

                while (true)
                {
                    object item = null;

                    lock (_syncObj)
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
                lock (_syncObj)
                {
                    currentTask = null;
                    currentThread = null;
                }
                _logger.Debug("Process Loop was aborted!");
                Thread.ResetAbort();
            }
        }

        private void ProcessItem(object item)
        {
            try
            {
                if (item is FeedUpdateSummary feedUpdate)
                    OnFeedUpdate(feedUpdate);
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
            lock (_syncObj)
            {
                _logger.Debug("STRATEGY START!");

                if (isStarted)
                    throw new InvalidOperationException("Cannot start: Strategy is already running!");

                isStarted = true;
                execStopFlag = false;
                stopTask = null;
                UpdateIndicatorScheduler(1);
                WakeUpWorker();
            }
        }

        public override Task Stop(bool quick)
        {
            lock (_syncObj)
            {
                _logger.Debug("STRATEGY STOP! qucik=" + quick);

                if (stopTask == null)
                    stopTask = DoStop(quick);
                return stopTask;
            }
        }

        public override void Abort()
        {
            lock (_syncObj)
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
                    _logger.Debug("STRATEGY ASYNC STOP!");
                    try
                    {
                        await b.InvokeAsyncStop();
                    }
                    finally
                    {
                        asyncStopDoneEvent?.TrySetResult(this);
                    }
                    _logger.Debug("STRATEGY ASYNC STOP DONE!");
                });

                await asyncStopDoneEvent.Task.ConfigureAwait(false); // wait async stop to end
                asyncStopDoneEvent = null;
            }

            bool aborted;
            lock (_syncObj)
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
                        _logger.Debug("STRATEGY CALL OnStop()!");
                        b.InvokeOnStop();
                    }
                    finally
                    {
                        lock (_syncObj)
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

                _logger.Debug("STRATEGY JOIN!");
                if (toWait != null)
                {
                    try
                    {
                        await toWait.ConfigureAwait(false); // wait current invoke to end
                    }
                    catch { } //we logging this case on ProcessLoop
                }
                _logger.Debug("STRATEGY DONE JOIN!");
            }

            lock (_syncObj)
            {
                ClearQueues();
                execStopFlag = false;
                isStarted = false;
                stopTask = null;
                UpdateIndicatorScheduler(-1);

                _logger.Debug("STRATEGY STOP COMPLETED!");
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
            {
                feedQueue.GetFeedUpdate(_feedUpdate);
                return _feedUpdate;
            }
            else
                return null;
        }


        private static void UpdateIndicatorScheduler(int changeCnt)
        {
            const int minThreads = 2;
            const int indicatorsPerThread = 100;

            try
            {
                lock (_indicatorScheduler)
                {
                    _activeIndicatorCnt += changeCnt;
                    var threadCnt = _activeIndicatorCnt / indicatorsPerThread;
                    if (threadCnt < minThreads)
                        threadCnt = minThreads;
                    else if (threadCnt > Environment.ProcessorCount / 2)
                        threadCnt = Environment.ProcessorCount / 2;

                    _indicatorScheduler.SetMaxDegreeOfParallelism(threadCnt);
                }
            }
            catch { }
        }
    }
}
