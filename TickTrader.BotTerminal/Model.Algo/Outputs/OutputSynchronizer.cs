using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Core;

namespace TickTrader.BotTerminal
{
    internal abstract class OutputSynchronizer
    {
        public abstract void Start(ITimeVectorRef baseVector);
        public abstract void Stop();
        public abstract bool ApproveAppend(DateTime itemTime, int itemIndex);
        public abstract int ApproveUpdate(DateTime itemTime, int itemIndex);
        public abstract void OnTruncate(int truncateSize);

        public virtual event Action<DateTime> AppendEmpty;

        public static OutputSynchronizer Null { get; } = new NullSynchronizer();
        public static OutputSynchronizer CreateShiftSynchronizer() => new ShiftSynchronizer();

        private class NullSynchronizer : OutputSynchronizer
        {
            public override void Start(ITimeVectorRef baseVector) { }
            public override void Stop() { }
            public override bool ApproveAppend(DateTime itemTime, int itemIndex) => true;
            public override int ApproveUpdate(DateTime itemTime, int itemIndex) => itemIndex;
            public override void OnTruncate(int truncateSize) { }

            public override event Action<DateTime> AppendEmpty { add { } remove { } }
        }

        private class ShiftSynchronizer : OutputSynchronizer
        {
            private bool _isInSync;
            private int _shift;
            private DateTime _baseTime;
            private ITimeVectorRef _baseVector;

            public override void Start(ITimeVectorRef baseVector)
            {
                if (baseVector.Count <= 0)
                    throw new InvalidOperationException("Reference vector is empty!");

                _baseVector = baseVector;
                _baseTime = baseVector.GetTimeAt(0);
                _isInSync = false;
            }

            public override void Stop()
            {
                _isInSync = false;
                _baseVector = null;
                _baseTime = DateTime.MaxValue;
                _shift = 0;
            }

            public override bool ApproveAppend(DateTime itemTime, int itemIndex)
            {
                if (_isInSync)
                    return true;

                if (itemTime >= _baseTime)
                {
                    // calculate shift
                    var baseIndex = GetBaseIndexByTime(itemTime);
                    _shift = baseIndex - itemIndex;

                    if (_shift > 0)
                        FillEmptySpace(_shift);

                    _isInSync = true;
                    return true;
                }

                return false;
            }

            public override int ApproveUpdate(DateTime itemTime, int itemIndex)
            {
                if (!_isInSync)
                    return -1;

                return itemIndex + _shift;
            }

            public override void OnTruncate(int truncateSize)
            {
                _shift += truncateSize;
            }

            private int GetBaseIndexByTime(DateTime time)
            {
                int i = 0;

                for (;i < _baseVector.Count; i++)
                {
                    if (time <= _baseVector.GetTimeAt(i))
                        break;
                }

                return i;
            }

            private void FillEmptySpace(int stopIndex)
            {
                for (int i = 0; i < stopIndex; i++)
                {
                    var time = _baseVector.GetTimeAt(i);
                    AppendEmpty(time);
                }
            }
        }
    }
}
