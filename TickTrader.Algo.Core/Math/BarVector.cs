using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.Core
{
    [Serializable]
    public class BarVector : IReadOnlyList<BarEntity>
    {
        private readonly List<BarEntity> _list = new List<BarEntity>();
        private readonly BarSequenceBuilder _builder;

        private BarVector(BarSequenceBuilder builder)
        {
            _builder = builder;

            _builder.BarOpened += (b) => _list.Add(b);
        }

        public BarVector(TimeFrames timeFrame)
            : this(BarSequenceBuilder.Create(timeFrame))
        {
        }

        public BarVector(ITimeSequenceRef masterSequence)
            : this(BarSequenceBuilder.Create(masterSequence))
        {
        }

        public BarVector(BarVector masterVector)
            : this(BarSequenceBuilder.Create(masterVector._builder))
        {
        }

        public BarEntity Last
        {
            get { return _list[_list.Count - 1]; }
            set { _list[_list.Count - 1] = value; }
        }

        public BarEntity First
        {
            get { return _list[0]; }
            set { _list[0] = value; }
        }

        public ITimeSequenceRef Ref => _builder;
        public TimeFrames TimeFrame => _builder.TimeFrame;

        public int Count => _list.Count;

        public BarEntity this[int index] => _list[index];

        public event Action<BarEntity> BarClosed { add { _builder.BarClosed += value; } remove { _builder.BarClosed -= value; } }

        public void AppendQuote(DateTime time, double price, double volume)
        {
            _builder.AppendQuote(time, price, volume);
        }

        public void AppendBar(BarEntity bar)
        {
            _builder.AppendBar(bar);
        }

        public void AppendBarPart(DateTime time, double open, double high, double low, double close, double volume)
        {
            _builder.AppendBarPart(time, open, high, low, close, volume);
        }

        public BarEntity[] ToArray()
        {
            return _list.ToArray();
        }

        public BarEntity[] RemoveFromStart(int count)
        {
            if (Count < count)
                throw new InvalidOperationException("Not enough bars in vector!");

            var range = new BarEntity[count];
            for (int i = 0; i < count; i++)
                range[i] = _list[i];
            _list.RemoveRange(0, count);
            return range;
        }

        public IEnumerator<BarEntity> GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _list.GetEnumerator();
        }
    }
}
