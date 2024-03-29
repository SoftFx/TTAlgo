﻿using System;
using System.Collections;
using System.Collections.Generic;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Core
{
    public class BarVector : IReadOnlyList<BarData>, ICollection<BarData>
    {
        private readonly List<BarData> _list = new List<BarData>();
        private readonly BarSequenceBuilder _builder;

        private BarVector(BarSequenceBuilder builder)
        {
            _builder = builder;

            _builder.BarOpened += (b) => _list.Add(b);
        }

        public BarVector(Feed.Types.Timeframe timeFrame)
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

        public BarData Last
        {
            get { return _list[_list.Count - 1]; }
            set { _list[_list.Count - 1] = value; }
        }

        public BarData First
        {
            get { return _list[0]; }
            set { _list[0] = value; }
        }

        public ITimeSequenceRef Ref => _builder;
        public Feed.Types.Timeframe TimeFrame => _builder.TimeFrame;

        public int Count => _list.Count;

        public BarData this[int index] => _list[index];

        public event Action<BarData> BarClosed { add { _builder.BarClosed += value; } remove { _builder.BarClosed -= value; } }

        public void AppendQuote(UtcTicks time, double price, double volume)
        {
            _builder.AppendQuote(time, price, volume);
        }

        public void AppendBarPart(BarData bar)
        {
            _builder.AppendBarPart(bar);
        }

        public BarData[] RemoveFromStart(int count)
        {
            if (Count < count)
                throw new InvalidOperationException("Not enough bars in vector!");

            var range = new BarData[count];
            for (int i = 0; i < count; i++)
                range[i] = _list[i];
            _list.RemoveRange(0, count);
            return range;
        }

        public void Close()
        {
            _builder.CloseSequence();
        }

        public IEnumerator<BarData> GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _list.GetEnumerator();
        }


        bool ICollection<BarData>.IsReadOnly => false;

        void ICollection<BarData>.Add(BarData item)
        {
            AppendBarPart(item);
        }

        void ICollection<BarData>.Clear()
        {
            _list.Clear();
        }

        bool ICollection<BarData>.Contains(BarData item)
        {
            return _list.Contains(item);
        }

        void ICollection<BarData>.CopyTo(BarData[] array, int arrayIndex)
        {
            _list.CopyTo(array, arrayIndex);
        }

        bool ICollection<BarData>.Remove(BarData item)
        {
            return _list.Remove(item);
        }
    }
}
