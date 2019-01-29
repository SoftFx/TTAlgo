using C5;
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
        private DelayedEventsQueue _delayedQueue = new DelayedEventsQueue();
        private Queue<Action<PluginBuilder>> _tradeQueue = new Queue<Action<PluginBuilder>>();
        private Queue<Action<PluginBuilder>> _eventQueue = new Queue<Action<PluginBuilder>>();
        private FeedEmulator _feed;
        private TimeSeriesAggregator _eventAggr = new TimeSeriesAggregator();
        private IBacktesterSettings _settings;
        private long _feedCount;
        private DateTime _timePoint;
        private long _safeTimePoint;
        private FeedReader _feedReader;
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
        }

        public DateTime UnsafeVirtualTimePoint { get { return _timePoint; } }
        public DateTime SafeVirtualTimePoint { get { lock (_sync) return _timePoint; } }
        public DateTime SlimUpdateVirtualTimePoint => new DateTime(Interlocked.Read(ref _safeTimePoint));
        public override int FeedQueueSize => 0;
        public bool IsStopPhase { get; private set; }
        public ScheduleEmulator Scheduler { get; } = new ScheduleEmulator();

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
            IsStopPhase = false;
            _eventAggr.Add(_delayedQueue);
            _eventAggr.Add(_feedReader);

            if (Scheduler.HasJobs)
            {
                Scheduler.Init(_timePoint);
                _eventAggr.Add(Scheduler);
            }
        }

        public void EmulateEventsWithFeed()
        {
            try
            {
                StartFeedRead();
                EmulateEvents();
            }
            finally
            {
                StopFeedRead();
            }
        }

        public void EmulateEvents()
        {
            _canceled = false;

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

        public void EnableStopPhase()
        {
            _fatalError = null;
            IsStopPhase = true;
        }

        public bool WarmupByBars(int barCount)
        {
            return Warmup((q, b, f, t) => b < barCount);
        }

        public bool WarmupByTimePeriod(TimeSpan period)
        {
            return Warmup((q, b, f, t) => t <= f + period);
        }

        public bool WarmupByQuotes(int quoteCount)
        {
            return Warmup((q, b, f, t) => q < quoteCount);
        }

        private bool Warmup(Func<int, int, DateTime, DateTime, bool> condition)
        {
            StartFeedRead();

            var warmupStart = _timePoint;
            var buider = _feed.GetBarBuilder(_settings.MainSymbol, _settings.MainTimeframe, BarPriceType.Bid);
            var tickCount = 1;

            while (true)
            {
                RateUpdate nextTick;

                if (!ReadNextFeed(out nextTick))
                {
                    LogWarmupFail(tickCount, buider.Count);
                    return false;
                }

                UpdateVirtualTimepoint(nextTick.Time);

                if (tickCount == 1)
                {
                    warmupStart = nextTick.Time;
                    LogWarmupStart();
                }

                tickCount++;

                if (!condition(tickCount, buider.Count - 1, warmupStart, _timePoint))
                    break;
            }

            var warmupEnd = _timePoint;

            LogWarmupEnd(tickCount, buider.Count - 1);
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
                if (_feedReader == null)
                    _feedReader = new FeedReader(_feed);
            }
        }

        private bool ReadNextFeed(out RateUpdate update)
        {
            if (_feedReader.IsCompeted)
            {
                update = default(RateUpdate);
                return false;
            }

            update = _feedReader.Take();

            return true;
        }

        private void StopFeedRead()
        {
            if (_feedReader != null)
            {
                _feedReader.Dispose();
                _feedReader = null;
            }
        }

        private void EmulateDelayed(TimeSpan delay, Action<PluginBuilder> invokeAction, bool isTrade)
        {
            _delayedQueue.Add(new TimeEvent(_timePoint + delay, isTrade, invokeAction));
        }

        private void UpdateVirtualTimepoint(DateTime newVal)
        {
            _timePoint = newVal;

            if (++_feedCount % 50 == 0)
                Interlocked.Exchange(ref _safeTimePoint, newVal.Ticks);
        }

        private void EmulateRateUpdate(RateUpdate rate)
        {
            var bufferUpdate = OnFeedUpdate(rate);
            RateUpdated?.Invoke(rate);
            _collector.OnRateUpdate(rate);

            if (bufferUpdate.ExtendedBy > 0)
                _collector.OnBufferExtended(bufferUpdate.ExtendedBy);

            var acc = Builder.Account;
            if (acc.IsMarginType)
                _collector.RegisterEquity(SafeVirtualTimePoint, acc.Equity, acc.Margin);
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
                    if (_tradeQueue.Count > 0)
                        return _tradeQueue.Dequeue();

                    bool isTrade;
                    var next = DequeueUpcoming(out isTrade);

                    if (next == null)
                        return null;

                    if (isTrade)
                        return next;

                    // if next item is not trade update just queue it

                    if (next is Action<PluginBuilder>)
                        _eventQueue.Enqueue((Action<PluginBuilder>)next);
                    else
                        _feedQueue.Enqueue((RateUpdate)next);
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
            if (_feedReader.IsCompeted && _delayedQueue.IsEmpty)
            {
                isTrade = false;
                return null;
            }

            var next = _eventAggr.Dequeue();
            UpdateVirtualTimepoint(next.Time);
            isTrade = next.IsTrade;
            return next.Content;
        }
    }
}
