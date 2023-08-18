using System;

namespace TickTrader.Algo.Core.Lib
{
    internal static class RoundingHelper
    {
        private const int MaxDigits = 12;
        private const double StepEps = 1e-12;

        private static readonly long[] Pow10Cache =
        {
            1L,
            10L,
            100L,
            1_000L,
            10_000L,
            100_000L,
            1_000_000L,
            10_000_000L,
            100_000_000L,
            1_000_000_000L,
            10_000_000_000L,
            100_000_000_000L,
            1_000_000_000_000L,
        };

        private static readonly double[] EpsCache =
        {
            1e-12, // 0
            1e-11, // 1
            1e-10, // 2
            1e-9,  // 3
            1e-8,  // 4
            1e-7,  // 5
            1e-6,  // 6
            1e-5,  // 7
            1e-4,  // 8
            1e-4,  // 9
            1e-3,  // 10
            1e-3,  // 11
            1e-2,  // 12
        };


        public static double Round(double val, int digits)
        {
            if (IsInvalid(val))
                return double.NaN;

            if (!TryGetRoundingConfig(val, digits, out var pow10, out var eps))
                return val;

            var numerator = GetNumerator(val, pow10, out var fracPart);
            if (CmpGte(fracPart, 0.5, eps))
                numerator++;
            else if (CmpLte(fracPart, -0.5, eps))
                numerator--;

            return ToDouble(numerator, pow10);
        }

        public static double Floor(double val, int digits)
        {
            if (IsInvalid(val))
                return double.NaN;

            if (!TryGetRoundingConfig(val, digits, out var pow10, out var eps))
                return val;

            var numerator = GetNumerator(val, pow10, out var fracPart);
            if (IsNegative(val))
            {
                if (!CmpE0(fracPart, eps))
                    numerator--;
            }
            else
            {
                if (CmpE(fracPart, 1, eps))
                    numerator++;
            }

            return ToDouble(numerator, pow10);
        }

        public static double Ceil(double val, int digits)
        {
            if (IsInvalid(val))
                return double.NaN;

            if (!TryGetRoundingConfig(val, digits, out var pow10, out var eps))
                return val;

            var numerator = GetNumerator(val, pow10, out var fracPart);
            if (IsNegative(val))
            {
                if (CmpE(fracPart, -1, eps))
                    numerator--;
            }
            else
            {
                if (!CmpE0(fracPart, eps))
                    numerator++;
            }

            return ToDouble(numerator, pow10);
        }

        public static double Round(double val, double step)
        {
            if (IsInvalid(val) || IsInvalid(step))
                return double.NaN;

            if (!TryGetRoundingConfig(val, step, out var pow10, out var eps))
                return val;

            var numerator = GetNumerator(val, step, pow10, out var fracPart, out var intStep);
            var halfStep = intStep / 2.0;
            if (CmpGte(fracPart, halfStep, eps))
                numerator += intStep;
            else if (CmpLte(fracPart, -halfStep, eps))
                numerator -= intStep;

            return ToDouble(numerator, pow10);
        }

        public static double Floor(double val, double step)
        {
            if (IsInvalid(val) || IsInvalid(step))
                return double.NaN;

            if (!TryGetRoundingConfig(val, step, out var pow10, out var eps))
                return val;

            var numerator = GetNumerator(val, step, pow10, out var fracPart, out var intStep);
            if (IsNegative(val))
            {
                if (!CmpE0(fracPart, eps))
                    numerator -= intStep;
            }
            else
            {
                if (CmpE(fracPart, intStep, eps))
                    numerator += intStep;
            }

            return ToDouble(numerator, pow10);
        }

        public static double Ceil(double val, double step)
        {
            if (IsInvalid(val) || IsInvalid(step))
                return double.NaN;

            if (!TryGetRoundingConfig(val, step, out var pow10, out var eps))
                return val;

            var numerator = GetNumerator(val, step, pow10, out var fracPart, out var intStep);
            if (IsNegative(val))
            {
                if (CmpE(fracPart, -intStep, eps))
                    numerator -= intStep;
            }
            else
            {
                if (!CmpE0(fracPart, eps))
                    numerator += intStep;
            }

            return ToDouble(numerator, pow10);
        }


        private static bool IsInvalid(double val)
        {
            return double.IsNaN(val) || double.IsInfinity(val);
        }

        private static bool TryGetRoundingConfig(double val, int digits, out long pow10, out double eps)
        {
            if (digits < 0 || digits > MaxDigits)
                throw new ArgumentOutOfRangeException(nameof(digits), digits, $"'{nameof(digits)}' is expected to be in range [0, {MaxDigits}]");

            var precision = GetExtraDecimals(val) + digits;
            if (precision > MaxDigits)
            {
                pow10 = 1; eps = double.MinValue;
                return false;
            }

            pow10 = Pow10Cache[digits];
            eps = EpsCache[precision];

            return true;
        }

        private static bool TryGetRoundingConfig(double val, double step, out long pow10, out double eps)
        {
            if (CmpGt(step, 1, StepEps) || CmpLte(step, 0, StepEps))
                throw new ArgumentOutOfRangeException(nameof(step), step, $"'{nameof(step)}' is expected to be in range (0; 1]");

            var digits = GetStepDigits(step);
            if (digits < 0 || digits > MaxDigits)
                throw new ArgumentException(nameof(step), $"'{nameof(step)}' digits are out of expected range");

            return TryGetRoundingConfig(val, digits, out pow10, out eps);
        }

        private static bool CmpE0(double a, double eps) => Math.Abs(a) < eps;

        private static bool CmpE(double a, double b, double eps) => Math.Abs(a - b) < eps;

        private static bool CmpLt(double a, double b, double eps) => a < b - eps;

        private static bool CmpLte(double a, double b, double eps) => a < b + eps;

        private static bool CmpGt(double a, double b, double eps) => a > b + eps;

        private static bool CmpGte(double a, double b, double eps) => a > b - eps;

        private static bool IsNegative(double a) => Math.Sign(a) < 0;

        private static int GetExtraDecimals(double d)
        {
            d = Math.Abs(d);
            if (d < 1e6)
            {
                if (d < 1e3)
                {
                    return d < 1e1 ? 0 : d < 1e2 ? 1 : 2;
                }
                else
                {
                    return d < 1e4 ? 3 : d < 1e5 ? 4 : 5;
                }
            }
            else if (d < 1e12)
            {
                if (d < 1e9)
                {
                    return d < 1e7 ? 6 : d < 1e8 ? 7 : 8;
                }
                else
                {
                    return d < 1e10 ? 9 : d < 1e11 ? 10 : 11;
                }
            }
            else
            {
                return d < 1e13 ? 12
                : d < 1e14 ? 13
                : d < 1e15 ? 14
                //: (int)Math.Log10(d);
                : 15; // extra not required
            }
        }

        private static int GetStepDigits(double step)
        {
            var digits = 0;
            while (digits <= MaxDigits && Math.Abs(Math.Truncate(step) - step) > StepEps)
            {
                step *= 10;
                digits++;
            }
            return digits;
        }

        private static double ToDouble(long numerator, long denominator) => ((double)numerator) / denominator;

        private static long GetNumerator(double val, long pow10, out double fracPart)
        {
            val *= pow10;
            var numerator = (long)val;
            fracPart = val - numerator;
            return numerator;
        }

        private static long GetNumerator(double val, double step, long pow10, out double fracPart, out long intStep)
        {
            val *= pow10;
            step *= pow10;
            intStep = (long)step;
            var numerator = intStep * (((long)val) / intStep);
            fracPart = val - numerator;
            return numerator;
        }
    }
}
