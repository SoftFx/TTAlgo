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
            ResultCache.AddLast(value);
        }

        protected override void InvokeUpdateLast(double value)
        {
            ResultCache.Last.Value = value;
        }

        protected override void SetCurrentResult()
        {
            Position = Shift > 0 ? 0 : -Shift;
            if (Accumulated > Math.Abs(Shift))
            {
                Result = ResultCache.First.Value;
                ResultCache.RemoveFirst();
            }
            else
            {
                Result = double.NaN;
            }
        }
    }
}
