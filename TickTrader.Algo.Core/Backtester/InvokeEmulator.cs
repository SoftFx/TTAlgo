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
    public enum EmulatorStates { WarmingUp, Running, Paused, Stopping, Stopped }

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
        private volatile bool _checkStateFlag;
        private BacktesterCollector _collector;
        private bool _normalStopFlag;
        private bool _canelRequested;
        private bool _pauseRequested;
        private Exception _fatalError;
        private Action _exStartAction;
        private Action _extStopAction;

        public InvokeEmulator(IBacktesterSettings settings, BacktesterCollector collector, FeedEmulator feed, Action exStartAction, Action extStopAction)
        {
            _settings = settings;
            _collector = collector;
            _collector.InvokeEmulator = this;
            _feed = feed;
            _exStartAction = exStartAction;
            _extStopAction = extStopAction;
        }

        public DateTime UnsafeVirtualTimePoint { get { return _timePoint; } }
        public DateTime SafeVirtualTimePoint { get { lock (_sync) return _timePoint; } }
        public DateTime SlimUpdateVirtualTimePoint => new DateTime(Interlocked.Read(ref _safeTimePoint));
        public override int FeedQueueSize => 0;
        internal bool IsStopPhase => State == EmulatorStates.Stopping;
        public ScheduleEmulator Scheduler { get; } = new ScheduleEmulator();
        public EmulatorStates State { get; private set; }

        public event Action<RateUpdate> RateUpdated;
        public event Action<EmulatorStates> StateUpdated;

        #region InvokeStartegy implementation

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
            //IsStopPhase = false;
            _eventAggr.Add(_delayedQueue);
            _eventAggr.Add(_feedReader);

            if (Scheduler.HasJobs)
            {
                Scheduler.Init(_timePoint);
                _eventAggr.Add(Scheduler);
            }
        }

        #endregion

        #region State Control

        public void Cancel()
        {
            lock (_sync)
            {
                if (State == EmulatorStates.Stopped) // can be canceled prior to execution due to CancellationToken
                    _canelRequested = true;

                if (State == EmulatorStates.Running || State == EmulatorStates.Paused)
                {
                    _canelRequested = true;
                    _pauseRequested = false;
                    _checkStateFlag = true;
                    Monitor.Pulse(_sync);
                }
            }
        }

        public void Pause()
        {
            lock (_sync)
            {
                if (State != EmulatorStates.Running || _canelRequested)
                    return;
                _pauseRequested = true;
                _checkStateFlag = true;
            }
        }

        public void Resume()
        {
            lock (_sync)
            {
                _pauseRequested = false;
                _checkStateFlag = false;
                Monitor.Pulse(_sync);
            }
        }

        private void ChangeState(EmulatorStates newState)
        {
            if (State != newState)
            {
                State = newState;
                StateUpdated?.Invoke(newState);
            }
        }

        #endregion

        public void EmulateExecution(int warmupValue, WarmupUnitTypes warmupUnits)
        {
            try
            {
                if (!WarmUp(warmupValue, warmupUnits))
                {
                    _collector.AddEvent(LogSeverities.Error, "No data for requested period!");
                    return;
                }
                _exStartAction();
                EmulateEvents();
                EmulateStop();
                StopFeedRead();
            }
            catch (OperationCanceledException)
            {
                _collector.AddEvent(LogSeverities.Error, "Testing canceled!");
                StopFeedRead();
                EmulateStop();
                throw;
            }
            catch (Exception ex)
            {
                StopFeedRead();
                EmulateStop();
                throw WrapException(ex);
            }
        }

        private void EmulateStop()
        {
            _extStopAction();
            //EmulateStop();
            //EnableStopPhase();
            EmulateEvents();
        }

        private void EmulateEvents()
        {
            lock (_sync)
            {
                _checkStateFlag = false;

                if (_canelRequested)
                {
                    _canelRequested = false;
                    ChangeState(EmulatorStates.Stopped);
                    throw new OperationCanceledException("Canceled.");
                }

                if (State == EmulatorStates.Stopped || State == EmulatorStates.WarmingUp)
                    ChangeState(EmulatorStates.Running);
            }

            try
            {
                while (!_normalStopFlag)
                {
                    if (_checkStateFlag)
                    {
                        lock (_sync)
                        {
                            if (_pauseRequested)
                            {
                                ChangeState(EmulatorStates.Paused);
                                while (_pauseRequested)
                                    Monitor.Wait(_sync);
                                if (!_canelRequested)
                                    ChangeState(EmulatorStates.Running);
                            }

                            if (_canelRequested)
                                throw new OperationCanceledException("Canceled.");
                        }
                    }

                    if (_fatalError != null)
                        throw _fatalError;

                    var nextItem = DequeueNext();

                    if (nextItem == null)
                        return;

                    ExecItem(nextItem);
                }
            }
            catch (Exception)
            {
                lock (_sync)
                {
                    if (State == EmulatorStates.Stopping)
                        ChangeState(EmulatorStates.Stopping);
                    else
                        ChangeState(EmulatorStates.Stopped);
                }
                throw;
            }
        }

        //public void EnableStopPhase()
        //{
        //    _fatalError = null;
        //    IsStopPhase = true;
        //}

        #region Warm-Up

        private bool WarmUp(int warmupValue, WarmupUnitTypes warmupUnits)
        {
            if (warmupValue <= 0)
                return true;

            lock (_sync) ChangeState(EmulatorStates.WarmingUp);

            if (warmupUnits == WarmupUnitTypes.Days)
                return WarmupByTimePeriod(TimeSpan.FromDays(warmupValue));
            else if (warmupUnits == WarmupUnitTypes.Hours)
                return WarmupByTimePeriod(TimeSpan.FromHours(warmupValue));
            else if (warmupUnits == WarmupUnitTypes.Bars)
                return WarmupByBars(warmupValue);
            else if (warmupUnits == WarmupUnitTypes.Ticks)
                return WarmupByQuotes(warmupValue);
            else
                throw new Exception("Unsupported warmup units: " + warmupUnits);
        }

        private bool WarmupByBars(int barCount)
        {
            return Warmup((q, b, f, t) => b < barCount);
        }

        private bool WarmupByTimePeriod(TimeSpan period)
        {
            return Warmup((q, b, f, t) => t <= f + period);
        }

        private bool WarmupByQuotes(int quoteCount)
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
                _collector.OnRateUpdate(nextTick);

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

        #endregion

        private void ExecItem(object item)
        {
            var action = item as Action<PluginBuilder>;
            if (action != null)
                action(Builder);
            else
                EmulateRateUpdate((RateUpdate)item);
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

        public bool StartFeedRead()
        {
            lock (_sync)
            {
                if (_feedReader == null)
                    _feedReader = new FeedReader(_feed);
                if (_feedReader.IsCompeted)
                    return false;
                UpdateVirtualTimepoint(_feedReader.NextOccurrance.Date);
                return true;
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

        public void StopFeedRead()
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
                _normalStopFlag = true;
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

        private Exception WrapException(Exception ex)
        {
            if (ex is AlgoException)
                return ex;

            return new AlgoException(ex.GetType().Name + ": " + ex.Message);
        }
    }
}
