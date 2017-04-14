using TickTrader.Algo.Api;

namespace TickTrader.Algo.TestCollection.Bots
{
    public enum ExitVariants { Init, OnStart, OnQuote, OnStop }

    [TradeBot(DisplayName = "[T] Exit Bot", Version = "1.0", Category = "Test Bot Routine",
        Description = "Call Exit function on specified moment of bot lifecycle")]
    public class ExitBot : TradeBot
    {
        [Parameter(DisplayName = "Exit on")]
        public ExitVariants ExitOn { get; set; }

        protected override void Init()
        {
            if (ExitOn == ExitVariants.Init)
                Exit();
        }

        protected override void OnStart()
        {
            if (ExitOn == ExitVariants.OnStart)
                Exit();
        }

        protected override void OnQuote(Quote quote)
        {
            if (ExitOn == ExitVariants.OnQuote)
                Exit();
        }

        protected override void OnStop()
        {
            if (ExitOn == ExitVariants.OnStop)
                Exit();
        }
    }
}
