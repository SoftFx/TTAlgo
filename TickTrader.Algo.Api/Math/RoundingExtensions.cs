namespace TickTrader.Algo.Api.Math
{
    public static class RoundingExtensions
    {
        private const int MaxDigits = 18;


        private static readonly long[] Pow10Cache =
        {
            1L,                     // 10e0
            10L,                    // 10e1
            100L,                   // 10e2
            1000L,                  // 10e3
            10000L,                 // 10e4
            100000L,                // 10e5
            1000000L,               // 10e6
            10000000L,              // 10e7
            100000000L,             // 10e8
            1000000000L,            // 10e9
            10000000000L,           // 10e10
            100000000000L,          // 10e11
            1000000000000L,         // 10e12
            10000000000000L,        // 10e13
            100000000000000L,       // 10e14
            1000000000000000L,      // 10e15
            10000000000000000L,     // 10e16
            100000000000000000L,    // 10e17
            1000000000000000000L,   // 10e18
        };


        /// <summary>
        /// Determines whether value has significant decimal part
        /// </summary>
        /// <returns>false if value has significant decimal part, true otherwise</returns>
        public static bool IsInteger(this double a)
        {
            return System.Math.Abs(System.Math.Round(a) - a).E(0);
        }

        /// <summary>
        /// Calculates number of digits in fractional part of value
        /// </summary>
        /// <returns>number of digits in fractional part</returns>
        public static int Digits(this double val)
        {
            var digits = 0;
            for (; !val.IsInteger(); val *= 10)
            {
                digits++;
            }
            return digits;
        }

        /// <summary>
        /// Rounds value to a certain amount of fractional digits
        /// </summary>
        /// <returns>Rounded value</returns>
        public static double Round(this double val, int digits)
        {
            if (double.IsNaN(val))
            {
                return double.NaN;
            }

            var fracPart = ToFraction(val, digits, out var numerator, out var denominator, out var negative);
            if (fracPart.Gte(0.5))
                numerator++;
            if (fracPart.Lte(-0.5))
                numerator--;
            return FractionToDouble(numerator, denominator);
        }

        /// <summary>
        /// Floors value to a certain amount of fractional digits
        /// </summary>
        /// <returns>Floored value</returns>
        public static double Floor(this double val, int digits)
        {
            if (double.IsNaN(val))
            {
                return double.NaN;
            }

            var fracPart = ToFraction(val, digits, out var numerator, out var denominator, out var negative);
            if (!negative && fracPart.E(1))
                numerator++;
            if (negative && !fracPart.E(0))
                numerator--;
            return FractionToDouble(numerator, denominator);
        }

        /// <summary>
        /// Ceils value to a certain amount of fractional digits
        /// </summary>
        /// <returns>Ceiled value</returns>
        public static double Ceil(this double val, int digits)
        {
            if (double.IsNaN(val))
            {
                return double.NaN;
            }

            var fracPart = ToFraction(val, digits, out var numerator, out var denominator, out var negative);
            if (!negative && !fracPart.E(0))
                numerator++;
            if (negative && fracPart.E(-1))
                numerator--;
            return FractionToDouble(numerator, denominator);
        }

        /// <summary>
        /// Rounds value to a certain step
        /// </summary>
        /// <returns>Rounded value</returns>
        public static double Round(this double val, double step)
        {
            if (double.IsNaN(val))
            {
                return double.NaN;
            }

            var fracPart = ToFraction(val, step, out var numerator, out var denominator, out var negative, out var intStep);
            if (fracPart.Gte(intStep / 2.0))
                numerator += intStep;
            if (fracPart.Lte(-intStep / 2.0))
                numerator -= intStep;
            return FractionToDouble(numerator, denominator);
        }

        /// <summary>
        /// Floors value to a certain step
        /// </summary>
        /// <returns>Floored value</returns>
        public static double Floor(this double val, double step)
        {
            if (double.IsNaN(val))
            {
                return double.NaN;
            }

            var fracPart = ToFraction(val, step, out var numerator, out var denominator, out var negative, out var intStep);
            if (!negative && fracPart.E(intStep))
                numerator += intStep;
            if (negative && !fracPart.E(0))
                numerator -= intStep;
            return FractionToDouble(numerator, denominator);
        }

        /// <summary>
        /// Ceils value to a certain step
        /// </summary>
        /// <returns>Ceiled value</returns>
        public static double Ceil(this double val, double step)
        {
            if (double.IsNaN(val))
            {
                return double.NaN;
            }

            var fracPart = ToFraction(val, step, out var numerator, out var denominator, out var negative, out var intStep);
            if (!negative && !fracPart.E(0))
                numerator += intStep;
            if (negative && fracPart.E(-intStep))
                numerator -= intStep;
            return FractionToDouble(numerator, denominator);
        }

        /// <summary>
        /// Rounds value to a certain amount of fractional digits
        /// </summary>
        /// <returns>Rounded value</returns>
        public static double? Round(this double? val, int digits)
        {
            return !val.HasValue ? default(double?) : val.Value.Round(digits);
        }

        /// <summary>
        /// Floors value to a certain amount of fractional digits
        /// </summary>
        /// <returns>Floored value</returns>
        public static double? Floor(this double? val, int digits)
        {
            return !val.HasValue ? default(double?) : val.Value.Floor(digits);
        }

        /// <summary>
        /// Ceils value to a certain amount of fractional digits
        /// </summary>
        /// <returns>Ceiled value</returns>
        public static double? Ceil(this double? val, int digits)
        {
            return !val.HasValue ? default(double?) : val.Value.Ceil(digits);
        }

        /// <summary>
        /// Rounds value to a certain step
        /// </summary>
        /// <returns>Rounded value</returns>
        public static double? Round(this double? val, double step)
        {
            return !val.HasValue ? default(double?) : val.Value.Round(step);
        }

        /// <summary>
        /// Floors value to a certain step
        /// </summary>
        /// <returns>Floored value</returns>
        public static double? Floor(this double? val, double step)
        {
            return !val.HasValue ? default(double?) : val.Value.Floor(step);
        }

        /// <summary>
        /// Ceils value to a certain step
        /// </summary>
        /// <returns>Ceiled value</returns>
        public static double? Ceil(this double? val, double step)
        {
            return !val.HasValue ? default(double?) : val.Value.Ceil(step);
        }


        private static double FractionToDouble(long numerator, long denominator)
        {
            return ((double)numerator) / denominator;
        }

        private static double ToFraction(double val, int digits, out long numerator, out long denominator, out bool negative)
        {
            digits = System.Math.Min(digits, MaxDigits);
            negative = val.Lt(0);
            val *= Pow10Cache[digits];
            denominator = Pow10Cache[digits];
            numerator = (long)val;
            return val - numerator;
        }

        private static double ToFraction(double val, double step, out long numerator, out long denominator, out bool negative, out long intStep)
        {
            var digits = System.Math.Min(step.Digits(), MaxDigits);
            negative = val.Lt(0);
            val *= Pow10Cache[digits];
            step *= Pow10Cache[digits];
            denominator = Pow10Cache[digits];
            intStep = (long)step;
            numerator = intStep * (((long)val) / intStep);
            return val - numerator;
        }
    }
}