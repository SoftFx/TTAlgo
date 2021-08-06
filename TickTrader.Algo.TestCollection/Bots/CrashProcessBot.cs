using System;
using System.Threading;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.TestCollection.Bots
{
    [TradeBot(DisplayName = "[T] Crash Process Bot", Version = "1.0.0", Category = "Test Plugin Routine", SetupMainSymbol = false,
        Description = "Crashes processes this bot is launched in")]
    public class CrashProcessBot : TradeBot
    {
        public enum CrashMethod { EnvironmentFailFast, ThreadPoolException, StackOverflow }

        public class SeppukuException : Exception { }


        public const string Seppuku = "Seppuku!!!";


        [Parameter(DefaultValue = CrashMethod.EnvironmentFailFast)]
        public CrashMethod Method { get; set; }


        protected override void OnStart()
        {
            switch (Method)
            {
                case CrashMethod.EnvironmentFailFast:
                    Environment.FailFast(Seppuku);
                    break;
                case CrashMethod.ThreadPoolException:
                    ThreadPool.QueueUserWorkItem(_ => throw new SeppukuException());
                    break;
                case CrashMethod.StackOverflow:
                    GenerateStackOverflow();
                    break;
                default:
                    PrintError("Unknown crash method");
                    break;
            }
        }


        private void GenerateStackOverflow()
        {
            Print($"42! == {BrokenFactorial(42)}");
        }

        private long BrokenFactorial(long n)
        {
            if (n == 1)
                return 1;

            return n * BrokenFactorial(n); // feature, not a bug :)
        }
    }
}
