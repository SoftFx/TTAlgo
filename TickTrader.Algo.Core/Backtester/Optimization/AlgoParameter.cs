using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api.Math;

namespace TickTrader.Algo.Core
{
    [Serializable]
    public class AlgoParameter
    {
        private double _cur;

        //private Stack<double> _stack = new Stack<double>();

        public bool IsStub { get; protected set; }

        public Type Type { get; }

        public double Step { get; }

        public double Min { get; }

        public double Max { get; }

        public bool IsEnd => Max.Lte(Current + Step);

        public int StepsCount => IsStub ? 1 : 1 + (int)(Math.Abs(Max - Min) / Math.Abs(Step));

        public double Current
        {
            get => _cur;

            protected set
            {
                if (_cur.E(value) || value.Gte(Max) || IsStub)
                    return;

                _cur = Math.Max(value, Min);
            }
        }


        public AlgoParameter()
        {
            IsStub = true;
        }

        public AlgoParameter(double curent)
        {
            Current = curent;
        }

        public AlgoParameter(double min, double max, double step)
        {
            Min = min;
            Max = max;

            Step = step;

            Current = min;
        }

        private AlgoParameter(double min, double max, double step, double cur) : this(min, max, step)
        {
            Current = cur;
        }

        public double Up(int kol = 1) => Current += Step * kol;

        public double Down(int kol = 1) => Current -= Step * kol;

        public void ResetParameter()
        {
            Current = Min;
        }

        public void Copy(AlgoParameter copy)
        {
            Current = copy.Current;
        }

        public AlgoParameter Copy() => new AlgoParameter(Min, Max, Step) { IsStub = IsStub };

        public AlgoParameter FullCopy() => new AlgoParameter(Min, Max, Step, Current) { IsStub = IsStub };

        public override string ToString() => IsStub ? "stub" : $"Min: {Min:F12}, Max: {Max:F12}, Step: {Step:F12}";

        public string ToShortString() => IsStub ? "stub" : $"{Current}";
    }
}
