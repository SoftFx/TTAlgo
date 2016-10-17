using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TestAlgoProject
{
    public enum Options { Option1, Option2 }

    public enum ExitVariants { Init, OnStart, OnQuote, OnStop }

    [TradeBot(DisplayName = "Exit Bot")]
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
