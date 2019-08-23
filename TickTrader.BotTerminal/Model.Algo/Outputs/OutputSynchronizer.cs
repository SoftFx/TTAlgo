using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Core;
using TickTrader.Algo.Core.Lib;

namespace TickTrader.BotTerminal
{
    internal class OutputSynchronizer<T>
    {
        private ITimeVectorRef _baseVector;
        private int _size;
        private T _emptyValue;

        public OutputSynchronizer(T emptyValue)
        {
            _emptyValue = emptyValue;
        }

        public Action<DateTime, T> DoAppend { get; set; }
        public Action<int, DateTime, T> DoUpdate { get; set; }

        public void Start(ITimeVectorRef baseVector)
        {
            _baseVector = baseVector;
            _size = 0;
        }

        public void Stop()
        {
        }

        public void AppendSnapshot(IEnumerable<OutputFixture<T>.Point> points)
        {
            int syncIndex = -1;

            foreach (var point in points)
            {
                var pointTime = point.TimeCoordinate.Value;

                if (syncIndex >= 0)
                {
                    while (true)
                    {
                        syncIndex++;

                        if (syncIndex >= _baseVector.Count)
                            return; // end of base vector -> exit

                        var baseTime = _baseVector[syncIndex];

                        if (baseTime == pointTime)
                        {
                            // hit -> append point
                            DoAppend?.Invoke(pointTime, point.Value);
                            _size++;
                            break; // take next point
                        }
                        else if (pointTime < baseTime)
                        {
                            // miss => base vector does not have this point -> skip point
                            break; // take next point
                        }
                        else // if (pointTime < baseTime)
                        {
                            // miss -> base vector contains point we dont have -> fill empty point
                            DoAppend?.Invoke(pointTime, _emptyValue);
                            _size++;
                        }
                    }
                }
                else if (pointTime >= _baseVector[0])
                    syncIndex = Append(point);
            }
        }

        public int Append(OutputFixture<T>.Point point)
        {
            var pointTime = point.TimeCoordinate.Value;
            var index = _baseVector.BinarySearch(pointTime, BinarySearchTypes.Exact);

            if (index >= 0)
            {
                FillEmptySpace(index - 1);
                DoAppend?.Invoke(pointTime, point.Value);
                _size++;
            }

            return index;
        }

        public void Update(OutputFixture<T>.Point point)
        {
            var pointTime = point.TimeCoordinate.Value;
            var index = _baseVector.BinarySearch(pointTime, BinarySearchTypes.Exact);
            if (index >= 0)
            {
                FillEmptySpace(index);
                DoUpdate?.Invoke(index, pointTime, point.Value);
            }
        }

        public void Truncate(int truncateSize)
        {
        }

        private void FillEmptySpace(int targetSize)
        {
            while (_size <= targetSize)
            {
                var pointTime = _baseVector[_size];
                DoAppend?.Invoke(pointTime, _emptyValue);
                _size++;
            }
        }
    }
}
