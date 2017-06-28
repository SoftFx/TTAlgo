using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.TestCollection.Bots
{
    public enum DelayVariants
    {
        Init, OnStart, OnStop
    }


    [TradeBot(DisplayName = "[T] Delay Bot", Version = "1.0", Category = "Test Bot Routine",
        Description = "Delays bot execution for configured number of milliseconds on specified moment of bot lifecycle")]
    public class DelayBot : TradeBot
    {
        public const int Pause = 20;


        [Parameter(DisplayName = "Delay on", DefaultValue = DelayVariants.OnStop)]
        public DelayVariants DelayOn { get; set; }

        [Parameter(DisplayName = "Delay (ms)", DefaultValue = 1000)]
        public int Delay { get; set; }

        protected override void Init()
        {
            if (DelayOn == DelayVariants.Init)
                ApplyDelay("Initializing");

            Status.WriteLine("Initialized");
            Status.Flush();
        }

        protected override void OnStart()
        {
            if (DelayOn == DelayVariants.OnStart)
                ApplyDelay("Starting");

            Status.WriteLine("Started");
            Status.Flush();
        }

        protected override void OnStop()
        {
            if (DelayOn == DelayVariants.OnStop)
                ApplyDelay("Stopping");

            Status.WriteLine("Stopped");
            Status.Flush();
        }


        private void ApplyDelay(string action)
        {
            Print($"Delay while {action}");
            Task.Delay(Delay).Wait();
        }
    }
}
