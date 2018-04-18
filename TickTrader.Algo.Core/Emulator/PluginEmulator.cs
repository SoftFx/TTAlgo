using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TickTrader.Algo.Core.Metadata;

namespace TickTrader.Algo.Core.Emulator
{
    public class PluginEmulator
    {
        private AlgoPluginDescriptor _descriptor;
        private SortedSet<IEmulatedEvent> _eventQueue;
        private Dictionary<string, IEnumerable<QuoteEntity>> _feedSources = new Dictionary<string, IEnumerable<QuoteEntity>>();

        public PluginEmulator(string pluginId)
        {
            _descriptor = AlgoPluginDescriptor.Get(pluginId);

            var eventComparer = Comparer<IEmulatedEvent>.Create((x, y) => x.Time.CompareTo(y.Time));
            _eventQueue = new SortedSet<IEmulatedEvent>(eventComparer);
        }

        public void AddFeedSource(string symbol, IEnumerable<QuoteEntity> src)
        {
            _feedSources.Add(symbol, src);
        }

        public void Emulate()
        {
            var feed = JoinFeedSources();

            foreach(var quote in feed)
            {
                while (_eventQueue.Count > 0 && quote.Time < _eventQueue.Max.Time)
                {
                    var nonFeedEvent = _eventQueue.Max;
                    _eventQueue.Remove(nonFeedEvent);

                    nonFeedEvent.Emulate();
                }

                EmulateQuote(quote);
            }

            foreach (var nonFeedEvent in _eventQueue)
                nonFeedEvent.Emulate();

            _eventQueue.Clear();
        }

        private void EmulateQuote(QuoteEntity quote)
        {
        }

        private IEnumerable<QuoteEntity> JoinFeedSources()
        {
            var enumartors = _feedSources.Values.Select(s => s.GetEnumerator()).ToList();

            while (enumartors.Count > 0)
            {
                var max = enumartors.MaxBy(e => e.Current);
                var nextQuote = max.Current;

                if (!max.MoveNext())
                    enumartors.Remove(max);

                yield return nextQuote;
            }
        }
    }

    internal interface IEmulatedEvent
    {
        DateTime Time { get; }
        void Emulate();
    }

    internal interface IEmulatorContext
    {
        void EnqueueEvent(IEmulatedEvent ev);
    }
}
