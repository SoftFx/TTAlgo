using System;
using System.Collections.Generic;
using System.Text;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.TestCollection.Bots
{

    [TradeBot(DisplayName = "[T] Close By Script", Version = "1.0", Category = "Test Orders", SetupMainSymbol = false)]
    internal class CloseByBot : TradeBot
    {
        [Parameter]
        public string FirstPosition { get; set; }


        [Parameter]
        public string SecondPosition { get; set; }


        protected override void OnStart()
        {
            var first = Account.Orders[FirstPosition];
            var second = Account.Orders[SecondPosition];

            if (!first.IsNull && !second.IsNull)
                CloseOrderBy(FirstPosition, SecondPosition);

            Exit();
        }
    }
}
