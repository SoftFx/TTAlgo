using System;

namespace TickTrader.Algo.Backtester
{
    public static class RangeParamSet
    {
        public class Double : RangeParamSet<double>
        {
            public override int Size
            {
                get
                {
                    if (Step == 0)
                        return 1;

                    return 1 + (int)(Math.Abs(Max - Min) / Math.Abs(Step));
                }
            }

            protected override double GetValue(int valNo)
            {
                var min = Math.Min(Min, Max);
                var step = Math.Abs(Step);

                return Math.Min(min + step * valNo, Max);
            }
        }


        public class Int32 : RangeParamSet<int>
        {
            public override int Size
            {
                get
                {
                    if (Step == 0)
                        return 1;

                    return 1 + ((Math.Abs(Max - Min) / Math.Abs(Step)));
                }
            }

            protected override int GetValue(int valNo)
            {
                var min = Math.Min(Min, Max);
                var step = Math.Abs(Step);

                return Math.Min(min + step * valNo, Max);
            }
        }
    }


    public abstract class RangeParamSet<T> : ParamSeekSet<T>
    {
        public T Min { get; set; }
        public T Max { get; set; }
        public T Step { get; set; }
    }
}
