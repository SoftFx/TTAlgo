using System;

namespace TickTrader.Algo.Indicators.Utility
{
    internal class SimpleShifter : ShifterBase
    {
        public SimpleShifter(int shift) : base(shift)
        {
        }

        protected override void InvokeAdd(double value)
        {
            if (Accumulated > Math.Abs(Shift) + 1)
            {
                ResultCache.RemoveFirst();
                Accumulated--;
            }
            if (Shift >= 0 || (Shift < 0 && Accumulated > -Shift))
            {
                ResultCache.AddLast(value);
            }
        }

        protected override void InvokeUpdateLast(double value)
        {
            if (Shift >= 0 || (Shift < 0 && Accumulated > -Shift))
            {
                ResultCache.Last.Value = value;
            }
        }

        protected override void SetCurrentResult()
        {
            Position = Shift > 0 ? 0 : -Shift;
            Result = Accumulated > Math.Abs(Shift) ? ResultCache.First.Value : double.NaN;
        }
    }
}
