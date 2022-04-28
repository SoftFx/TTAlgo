using System;
using System.Runtime.InteropServices;
using System.Threading;
using TickTrader.Algo.Domain;

namespace TestEnviroment
{
    public static class RandomGenerator
    {
        private const string PossibleChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

        private static readonly ThreadLocal<Random> _random = new(() => new Random(134134278));


        public static bool GetRandomBool() => _random.Value.Next(0, 2) > 0;

        public static int GetRandomInt(int min = -100, int max = 100, bool positive = false) =>
            _random.Value.Next(positive ? 0 : min, max);

        public static byte GetRandomByte(byte min = 0, byte max = 8) =>
            (byte)_random.Value.Next(min, max);

        public static double GetRandomDouble(double min = -100.0, double max = 100.0) =>
            _random.Value.NextDouble() * (GetRandomBool() ? min : max);

        public static double GetDouble() => _random.Value.NextDouble();

        public static string GetRandomString(int size = 8)
        {
            var stringChars = new char[size];

            for (int i = 0; i < size; i++)
                stringChars[i] = PossibleChars[_random.Value.Next(PossibleChars.Length)];

            return new string(stringChars);
        }

        public static byte[] GetRandomBytes(int size = 8)
        {
            var bytes = new byte[size];

            for (int i = 0; i < size; i++)
                bytes[i] = (byte)_random.Value.Next(0, 255);

            return bytes;
        }

        public static BarData GetBarData(DateTime open, DateTime close)
        {
            return new BarData
            {
                OpenTimeRaw = open.Ticks,
                CloseTimeRaw = close.Ticks,
                Open = GetRandomDouble(0.5, 1.0),
                Close = GetRandomDouble(0.5, 1.0),
                High = GetRandomDouble(1.1, 2.0),
                Low = GetRandomDouble(0.1, 0.4),
            };
        }

        public static QuoteInfo GetTick(string symbol, DateTime time)
        {
            return new QuoteInfo(symbol, time, GetRandomDouble(0.1, 0.5), GetRandomDouble(0.5, 0.6));
        }

        public static QuoteInfo GetTickL2(string symbol, DateTime time)
        {
            var bidsBand = GetQuoteBand();
            var asksBand = GetQuoteBand();

            var data = new QuoteData
            {
                UtcTicks = time.ToUniversalTime().Ticks,
                IsBidIndicative = bidsBand.Length > 0,
                IsAskIndicative = asksBand.Length > 0,

                BidBytes = ByteStringHelper.CopyFromUglyHack(MemoryMarshal.Cast<QuoteBand, byte>(bidsBand[..])),
                AskBytes = ByteStringHelper.CopyFromUglyHack(MemoryMarshal.Cast<QuoteBand, byte>(asksBand[..])),
            };

            return QuoteInfo.Create(symbol, data);
        }

        private static Span<QuoteBand> GetQuoteBand()
        {
            var length = GetRandomInt(0, 100);

            var bands = new QuoteBand[length];

            for (int i = 0; i < length; ++i)
                bands[i] = new QuoteBand(GetRandomDouble(0.1, 0.5), GetRandomDouble(0.1, 0.5));

            return bands;
        }
    }
}
