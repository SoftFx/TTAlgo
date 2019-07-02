using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Core
{
    public static class RangeParamSet
    {
        public class Double : RangeParamSet<double>
        {
            public override int Size => (int)(Math.Abs(Max - Min) / Step);
            protected override double GetValue(int valNo) => Min + Step * valNo;
        }

        public class Int32 : RangeParamSet<int>
        {
            public override int Size => (Math.Abs(Max - Min) / Step);
            protected override int GetValue(int valNo) => Min + Step * valNo;
        }
    }

    public abstract class RangeParamSet<T> : ParamSeekSet<T>
    {
        public T Min { get; set; }
        public T Max { get; set; }
        public T Step { get; set; }
    }
}
