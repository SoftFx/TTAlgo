using System.Collections.Generic;
using System.Threading.Channels;

namespace TickTrader.Algo.Async.Actors
{
    /// <summary>
    /// Not thread-safe. Should be used only in actors
    /// </summary>
    public class ActorEventSource<T>
    {
        private readonly LinkedList<WriterSub> _subs = new LinkedList<WriterSub>();


        public void DispatchEvent(T item)
        {
            for  (var subNode = _subs.First; subNode != null;)
            {
                var sub = subNode.Value;
                var nextNode = subNode.Next;
                if (!sub.SendEvent(item))
                {
                    sub.Dispose();
                    _subs.Remove(subNode);
                }
                subNode = nextNode;
            }
        }

        public void Subscribe(ChannelWriter<T> writer)
        {
            _subs.AddLast(new WriterSub(writer));
        }


        private class WriterSub
        {
            private readonly ChannelWriter<T> _writer;


            public WriterSub(ChannelWriter<T> writer)
            {
                _writer = writer;
            }


            public void Dispose() => _writer.TryComplete();

            public bool SendEvent(T item) => _writer.TryWrite(item);
        }
    }
}
