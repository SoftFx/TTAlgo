using System;
using System.Collections.Generic;

namespace TickTrader.Algo.Indicators.Utility
{
    internal abstract class ShifterBase : IShift
    {
        protected LinkedList<double> ResultCache;

        public int Shift { get; private set; }

        public int Accumulated { get; protected set; }
        public int Position { get; protected set; }
        public double Result { get; protected set; }

        internal ShifterBase(int shift)
        {
            Shift = shift;
        }

        public void Init()
        {
            Accumulated = 0;
            Position = 0;
            Result = double.NaN;
            ResultCache = new LinkedList<double>();
        }

        public void Reset()
        {
            Accumulated = 0;
            Position = 0;
            Result = double.NaN;
            ResultCache.Clear();
        }

        protected abstract void InvokeAdd(double value);
        protected abstract void InvokeUpdateLast(double value);
        protected abstract void SetCurrentResult();

        public void Add(double value)
        {
            Accumulated++;
            InvokeAdd(value);
            SetCurrentResult();
        }

        public void UpdateLast(double value)
        {
            if (Accumulated == 0)
            {
                throw new Exception("Last element doesn't exists.");
            }
            InvokeUpdateLast(value);
            SetCurrentResult();
        }
    }
}
