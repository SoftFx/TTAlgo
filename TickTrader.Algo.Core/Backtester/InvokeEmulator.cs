using C5;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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
        private long _feedCount;
        private DateTime _timePoint;
        private long _safeTimePoint;
        private IEnumerator<QuoteEntity> _eFeed;

        public InvokeEmulator()
        {
            //var eventComparer = Comparer<EmulatedAction>.Create((x, y) => x.Time.CompareTo(y.Time));
            //_delayedQueue = new  C5.IntervalHeap<EmulatedAction>(eventComparer);
        }

        public DateTime VirtualTimePoint { get { lock (_sync) return _timePoint; } }
        public DateTime SafeVirtualTimePoint => new DateTime(Interlocked.Read(ref _safeTimePoint));
        public override int FeedQueueSize => 0;

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
            var item = DequeueNext();
        }

        public override void Start()
        {
            _feed = (FeedEmulator)FStartegy.Feed;
        }

        public void EmulateEventsFlow(DateTime startTimePoint, CancellationToken cToken)
        {
            lock (_sync)
            {
                _eFeed = _feed.GetFeedStream().JoinPages();
                if (!_eFeed.MoveNext())
                    _eFeed = null;

                UpdateVirtualTimepoint(startTimePoint);
            }

            try
            {
                while (true)
                {
                    var nextItem = DequeueNext();

                    if (nextItem == null)
                        return;

                    var action = nextItem as Action<PluginBuilder>;
                    if (action != null)
                        action(Builder);
                    else
                        EmulateQuote((QuoteEntity)nextItem);
                }
            }
            finally
            {
                if (_eFeed != null)
                    _eFeed.Dispose();
            }


            //foreach (var quote in ReadFeedStream())
            //{
            //    while (true)
            //    {
            //        Action<PluginBuilder> toInvoke;

            //        lock (_sync)
            //        {
            //            if (_invokeQueue.Count == 0)
            //                break;

            //            toInvoke = _invokeQueue.Dequeue();
            //        }

            //        toInvoke.Invoke(Builder);
            //    }

            //    EmulateQuote(quote);

            //    lock (_sync) UpdateVirtualTimepoint(quote.CreatingTime);
            //}

            //while (true)
            //{
            //    Action<PluginBuilder> toInvoke;

            //    lock (_sync)
            //    {
            //        if (_invokeQueue.Count > 0)
            //            toInvoke = _invokeQueue.Dequeue();
            //        else if (_delayedQueue.Count > 0)
            //            toInvoke = _delayedQueue.DeleteMin().Action;
            //        else
            //            break;
            //    }

            //    toInvoke.Invoke(Builder);
            //}
        }

        public void EmulateDelayedInvoke(TimeSpan delay, Action<PluginBuilder> invokeAction)
        {
            EmulateDelayed(delay, invokeAction, true);
        }

        public void EmulateDelayedTrade(TimeSpan delay, Action<PluginBuilder> invokeAction)
        {
            EmulateDelayed(delay, invokeAction, true);
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

        private IEnumerable<QuoteEntity> ReadFeedStream()
        {
            var e = _feed.GetFeedStream();

            while (true)
            {
                var page = e.GetNextPage();
                if (page.Count == 0)
                    yield break;

                foreach (var q in page)
                    yield return q;
            }
        }

        private void EmulateQuote(QuoteEntity quote)
        {
            var update = FStartegy.InvokeAggregate(null, quote);
            OnFeedUpdate(update);
        }

        public override Task Stop(bool quick)
        {
            return Task.FromResult(true);
        }

        private object DequeueNext()
        {
            lock (_sync)
            {
                if (_eventQueue.Count > 0)
                    return _eventQueue.Dequeue();
                else if (_tradeQueue.Count > 0)
                    return _tradeQueue.Dequeue();
                else
                {
                    if (_delayedQueue.Count > 0)
                    {
                        if (_eFeed == null || _eFeed.Current.Time >= _delayedQueue.FindMin().Time)
                        {
                            var delayed = _delayedQueue.DeleteMin();
                            UpdateVirtualTimepoint(delayed.Time);
                            return delayed.Action;
                        }
                    }

                    if (_eFeed == null || !_eFeed.MoveNext())
                        return null;

                    var q = _eFeed.Current;
                    UpdateVirtualTimepoint(q.Time);
                    return q;
                }
            }
        }
    }

    internal struct EmulatedAction : IComparable<EmulatedAction>
    {
        public DateTime Time { get; set; }
        public bool IsTrade { get; set; }
        public Action<PluginBuilder> Action { get; set; }

        public int CompareTo(EmulatedAction other)
        {
            return other.Time.CompareTo(Time);
        }
    }
}
