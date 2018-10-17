﻿using C5;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TickTrader.Algo.Api;
using TickTrader.Algo.Core.Lib;

namespace TickTrader.Algo.Core
{
    internal class InvokeEmulator : InvokeStartegy
    {
        private object _sync = new object();
        private FeedQueue _feedQueue;
        private IPriorityQueue<EmulatedAction> _delayedQueue = new C5.IntervalHeap<EmulatedAction>();
        private Queue<Action<PluginBuilder>> _tradeQueue = new Queue<Action<PluginBuilder>>();
        private Queue<Action<PluginBuilder>> _eventQueue = new Queue<Action<PluginBuilder>>();
        private FeedEmulator _feed;
        private IBacktesterSettings _settings;
        private long _feedCount;
        private DateTime _timePoint;
        private long _safeTimePoint;
        private IEnumerator<RateUpdate> _eFeed;
        private volatile bool _canceled;
        private BacktesterCollector _collector;
        private bool _stopFlag;
        private Exception _fatalError;

        public InvokeEmulator(IBacktesterSettings settings, BacktesterCollector collector, FeedEmulator feed)
        {
            _settings = settings;
            _collector = collector;
            _collector.InvokeEmulator = this;
            _feed = feed;
            //var eventComparer = Comparer<EmulatedAction>.Create((x, y) => x.Time.CompareTo(y.Time));
            //_delayedQueue = new  C5.IntervalHeap<EmulatedAction>(eventComparer);
        }

        public DateTime VirtualTimePoint { get { lock (_sync) return _timePoint; } }
        public DateTime SafeVirtualTimePoint => new DateTime(Interlocked.Read(ref _safeTimePoint));
        public override int FeedQueueSize => 0;

        public event Action<RateUpdate> RateUpdated;

        protected override void OnInit()
        {
            _feedQueue = new FeedQueue(FStartegy);
        }

        public override void Abort()
        {
        }

        public override void EnqueueCustomInvoke(Action<PluginBuilder> a)
        {
            _eventQueue.Enqueue(a);
        }

        public override void EnqueueEvent(Action<PluginBuilder> a)
        {
            _eventQueue.Enqueue(a);
        }

        public override void EnqueueQuote(QuoteEntity update)
        {
            throw new InvalidOperationException("InvokeEmulator does not accept quote updates!");
        }

        public override void EnqueueTradeUpdate(Action<PluginBuilder> a)
        {
            _tradeQueue.Enqueue(a);
        }

        public override void ProcessNextTrade()
        {
            var item = DequeueNextTrade();

            if (item == null)
                throw new Exception("Detected empty queue while ProcessNextTrade()!");

            ExecItem(item);
        }

        public override void Start()
        {
        }

        public void EmulateEventsFlow()
        {
            try
            {
                StartFeedRead();

                while (!_stopFlag)
                {
                    if (_canceled)
                        throw new OperationCanceledException("Canceled.");

                    if (_fatalError != null)
                        throw _fatalError;

                    var nextItem = DequeueNext();

                    if (nextItem == null)
                        return;

                    ExecItem(nextItem);
                }
            }
            finally
            {
                StopFeedRead();
            }
        }

        public bool WarmupByBars(int barCount)
        {
            return Warmup((q, b, f, t) => b <= barCount);
        }

        public bool WarmupByTimePeriod(TimeSpan period)
        {
            return Warmup((q, b, f, t) => t <= f + period);
        }

        public bool WarmupByQuotes(int quoteCount)
        {
            return Warmup((q, b, f, t) => q <= quoteCount);
        }

        private bool Warmup(Func<int, int, DateTime, DateTime, bool> condition)
        {
            StartFeedRead();

            if (_eFeed == null)
            {
                LogWarmupFail(0, 0);
                return false;
            }

            var warmupStart = _eFeed.Current.Time;

            UpdateVirtualTimepoint(warmupStart);
            LogWarmupStart();

            var buider = _feed.GetBarBuilder(_settings.MainSymbol, _settings.MainTimeframe, BarPriceType.Bid);
            var tickCount = 0;

            while (true)
            {
                if (!_eFeed.MoveNext())
                {
                    LogWarmupFail(tickCount, buider.Count);
                    return false;
                }

                UpdateVirtualTimepoint(_eFeed.Current.Time);

                if (!condition(tickCount, buider.Count, warmupStart, _timePoint))
                    break;

                tickCount++;
            }

            var warmupEnd = _timePoint;

            LogWarmupEnd(tickCount, buider.Count);
            return true;
        }

        private void LogWarmupStart()
        {
            _collector.AddEvent(LogSeverities.Info, "Start warmup");
        }

        private void LogWarmupFail(int ticksCount, int barCount)
        {
            _collector.AddEvent(LogSeverities.Error, string.Format("Not enough data for warmup! Loaded {0} bars ({1} quotes) during warmup attempt.", barCount, ticksCount));
        }

        private void LogWarmupEnd(int tickCount, int barCount)
        {
            _collector.AddEvent(LogSeverities.Info, string.Format("Warmup completed. Loaded {0} bars ({1} quotes) during warmup.", barCount, tickCount));
        }

        private void ExecItem(object item)
        {
            var action = item as Action<PluginBuilder>;
            if (action != null)
                action(Builder);
            else
                EmulateRateUpdate((RateUpdate)item);
        }

        public void Cancel()
        {
            _canceled = true;
        }

        public void EmulateDelayedInvoke(TimeSpan delay, Action<PluginBuilder> invokeAction, bool isTradeAction)
        {
            EmulateDelayed(delay, invokeAction, isTradeAction);
        }

        public void EmulateDelayedTrade(TimeSpan delay, Action<PluginBuilder> invokeAction)
        {
            EmulateDelayed(delay, invokeAction, true);
        }

        public Task EmulateAsyncDelay(TimeSpan delay, bool isTradeDelay)
        {
            var handler = new TaskCompletionSource<object>();
            EmulateDelayed(delay, b => handler.SetResult(null), isTradeDelay);
            return handler.Task;
        }

        public void SetFatalError(Exception error)
        {
            lock (_sync)
            {
                if (_fatalError != error)
                    _fatalError = error;
            }
        }

        private void StartFeedRead()
        {
            lock (_sync)
            {
                if (_eFeed == null)
                {
                    _eFeed = _feed.GetFeedStream().GetEnumerator();
                    if (!_eFeed.MoveNext())
                        _eFeed = null;

                    UpdateVirtualTimepoint(_settings.EmulationPeriodStart ?? DateTime.MinValue);
                }
            }
        }

        private void StopFeedRead()
        {
            _eFeed?.Dispose();
            _eFeed = null;
        }

        private void EmulateDelayed(TimeSpan delay, Action<PluginBuilder> invokeAction, bool isTrade)
        {
            _delayedQueue.Add(new EmulatedAction { Action = invokeAction, Time = _timePoint + delay, IsTrade = isTrade });
        }

        private void UpdateVirtualTimepoint(DateTime newVal)
        {
            _timePoint = newVal;

            if (++_feedCount % 50 == 0)
                Interlocked.Exchange(ref _safeTimePoint, newVal.Ticks);

            // enqueue triggered delays
            while (_delayedQueue.Count > 0 && newVal < _delayedQueue.FindMin().Time)
            {
                var delayedItem = _delayedQueue.DeleteMin();
                if (delayedItem.IsTrade)
                    _tradeQueue.Enqueue(delayedItem.Action);
                else
                    _eventQueue.Enqueue(delayedItem.Action);
            }
        }

        //private IEnumerable<QuoteEntity> ReadFeedStream()
        //{
        //    var e = _feed.GetFeedStream();

        //    while (true)
        //    {
        //        var page = e.GetNextPage();
        //        if (page.Count == 0)
        //            yield break;

        //        foreach (var q in page)
        //            yield return q;
        //    }
        //}

        private void EmulateRateUpdate(RateUpdate rate)
        {
            var bufferUpdate = OnFeedUpdate(rate);
            RateUpdated?.Invoke(rate);
            _collector.OnRateUpdate(rate);

            if (bufferUpdate.ExtendedBy > 0)
                _collector.OnBufferExtended(bufferUpdate.ExtendedBy);

            var acc = Builder.Account;
            if (acc.IsMarginType)
                _collector.RegisterEquity(VirtualTimePoint, acc.Equity, acc.Margin);
        }

        public override Task Stop(bool quick)
        {
            var stopEvent = new TaskCompletionSource<object>();

            _eventQueue.Enqueue(b =>
            {
                b.InvokeOnStop();
                _stopFlag = true;
                stopEvent.SetResult(this);
            });

            return stopEvent.Task;
        }

        private object DequeueNextTrade()
        {
            lock (_sync)
            {
                while (true)
                {
                    bool isTrade;
                    var next = DequeueUpcoming(out isTrade);

                    if(next == null)
                        return null;

                    if (isTrade)
                        return next;

                    // if next item is not trade update just queue it

                    if (next is Action<PluginBuilder>)
                        _eventQueue.Enqueue((Action<PluginBuilder>)next);
                    else
                        _feedQueue.Enqueue((BarRateUpdate)next);
                }
            }
        }

        private object DequeueNext()
        {
            lock (_sync)
            {
                if (_eventQueue.Count > 0)
                    return _eventQueue.Dequeue();
                else if (_tradeQueue.Count > 0)
                    return _tradeQueue.Dequeue();
                else if (_feedQueue.Count > 0)
                    return _feedQueue.Dequeue();
                else
                    return DequeueUpcoming(out _);
            }
        }

        private object DequeueUpcoming(out bool isTrade)
        {
            var delayed = DequeueDelayed(out isTrade);
            if (delayed != null)
                return delayed;

            isTrade = false;

            if (_eFeed == null || !_eFeed.MoveNext())
            {
                _eFeed = null;
                return DequeueDelayed(out isTrade);
            }

            var q = _eFeed.Current;
            UpdateVirtualTimepoint(q.Time);
            return q;
        }

        private object DequeueDelayed(out bool isTrade)
        {
            if (_delayedQueue.Count > 0)
            {
                if (_eFeed == null || _eFeed.Current.Time >= _delayedQueue.FindMin().Time)
                {
                    var delayed = _delayedQueue.DeleteMin();
                    UpdateVirtualTimepoint(delayed.Time);
                    isTrade = delayed.IsTrade;
                    return delayed.Action;
                }
            }
            isTrade = false;
            return null;
        }
    }

    internal struct EmulatedAction : IComparable<EmulatedAction>
    {
        public DateTime Time { get; set; }
        public bool IsTrade { get; set; }
        public Action<PluginBuilder> Action { get; set; }

        public int CompareTo(EmulatedAction other)
        {
            return Time.CompareTo(other.Time);
        }
    }
}
