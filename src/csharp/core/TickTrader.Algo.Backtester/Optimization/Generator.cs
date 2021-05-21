using System;

namespace TickTrader.Algo.Backtester
{
    public class Generator
    {
        private readonly Random _random;

        public Generator()
        {
            _random = new Random((int)DateTime.Now.Ticks);
        }


        public double GetPart() => _random.NextDouble();

        public double GetDouble(double max) => GetPart() * max;

        public int GetInt(int min, int max) => _random.Next(min, max);

        public int GetInt(int max) => _random.Next(max);

        public int GetInt() => _random.Next();

        public bool GetBool() => _random.Next(2) == 1;

        public double GetDoubleInRange(double min, double max) => min + GetPart() * (max - min);

        //public double GetDouble(Parameter param) =>
        //    GetWithCondition(() => GetDoubleInRange(param.Min, param.Max),
        //    (x) => param.Min.Lte(x) && param.Max.Gte(x));

        public (int, int) GetRandomPair(int max)
        {
            var first = GetInt(max);
            var second = GetIntWithCondition(max, (x) => first != x);

            return (first, second);
        }

        public double GetNormalNumber(double mean, double variance)
        {
            while (true)
            {
                double e1 = GetExponentialNumber(), e2 = GetExponentialNumber();

                if (e2 > Math.Pow(e1 - 1, 2) / 2)
                    return mean + (GetPart() < 0.5 ? -1 : 1) * Math.Abs(e1) *
                        Math.Sqrt(variance);
            }
        }

        public double GetCauchyNumber() => Math.Tan(Math.PI * (GetPart() - 0.5));

        private double GetExponentialNumber() => -Math.Log(GetPart());

        public double GetDoubleWithCondition(double maxValue, Predicate<double> pred) => GetWithCondition(() => GetDouble(maxValue), pred);

        public int GetIntWithCondition(int maxValue, Predicate<int> pred) => GetWithCondition(() => GetInt(maxValue), pred);

        private static T GetWithCondition<T>(Func<T> func, Predicate<T> pred)
        {
            T value;

            do
            {
                value = func();
            }
            while (!pred(value));

            return value;
        }
    }
}
