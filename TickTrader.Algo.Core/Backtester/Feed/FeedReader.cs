using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Core
{
    internal class FeedReader : IEnumerable<IRateInfo>, IDisposable
    {
        private Task _worker;
        private FlipGate<IRateInfo> _gate = new FlipGate<IRateInfo>(1000);

        public FeedReader(IEnumerable<SeriesReader> sources)
        {
            _worker = Task.Factory.StartNew(() => ReadLoop(sources), TaskCreationOptions.LongRunning);
        }

        public Exception Fault { get; private set; }
        public bool HasFailed => Fault != null;

        public IEnumerator<IRateInfo> GetEnumerator()
        {
            return _gate.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private void ReadLoop(IEnumerable<SeriesReader> sources)
        {
            var streams = sources.ToList();

            try
            {
                foreach (var e in streams.ToList())
                {
                    e.Start();

                    if (!e.MoveNext())
                    {
                        streams.Remove(e);
                        e.Stop();
                    }
                }

                while (streams.Count > 0)
                {
                    var min = GetMin(streams);
                    var nextQuote = min.Current;

                    if (!min.MoveNext())
                    {
                        streams.Remove(min);
                        min.Stop();
                    }

                    if (!_gate.Write(nextQuote))
                        break;
                }
            }
            catch(Exception ex)
            {
                Fault = ex;
            }

            foreach (var src in streams)
                src.Stop();

            _gate.CompleteWrite();
        }

        private SeriesReader GetMin(List<SeriesReader> readers)
        {
            DateTime minTime = DateTime.MaxValue;
            SeriesReader minReader = null;

            for (int i = 0; i < readers.Count; i++)
            {
                var reader = readers[i];
                var readerTime = reader.Current.Time;

                if (minTime > readerTime)
                {
                    minReader = reader;
                    minTime = readerTime;
                }
            }

            return minReader;
        }

        public void Dispose()
        {
            _gate.Close();
            _worker.Wait();
        }
    }
}
