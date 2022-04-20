using System;
using TickTrader.Algo.Domain;

namespace TestEnviroment
{
    public static class RandomGenerator
    {
        private const string PossibleChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

        private static readonly Random _random = new(134134278);


        public static bool GetRandomBool() => _random.Next(0, 2) > 0;

        public static int GetRandomInt(int min = -100, int max = 100, bool positive = false) =>
            _random.Next(positive ? 0 : min, max);

        public static byte GetRandomByte(byte min = 0, byte max = 8) =>
            (byte)_random.Next(min, max);

        public static double GetRandomDouble(double min = -100.0, double max = 100.0) =>
            _random.NextDouble() * (GetRandomBool() ? min : max);

        public static double GetDouble() => _random.NextDouble();

        public static string GetRandomString(int size = 8)
        {
            var stringChars = new char[size];

            for (int i = 0; i < size; i++)
                stringChars[i] = PossibleChars[_random.Next(PossibleChars.Length)];

            return new string(stringChars);
        }

        public static byte[] GetRandomBytes(int size = 8)
        {
            var bytes = new byte[size];

            for (int i = 0; i < size; i++)
                bytes[i] = (byte)_random.Next(0, 255);

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
    }
}
