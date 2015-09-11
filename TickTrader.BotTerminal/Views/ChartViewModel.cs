using Caliburn.Micro;
using SoftFX.Extended;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.BotTerminal
{
    class ChartViewModel : Screen
    {
        public ChartViewModel(string symbol, ConnectionModel connection)
        {
            this.Symbol = symbol;
            this.DisplayName = symbol;

            var response = connection.FeedProxy.Server.GetHistoryBars(
                symbol, DateTime.Now + TimeSpan.FromDays(1), 
                -4000, SoftFX.Extended.PriceType.Ask, BarPeriod.H1);

            this.Data = new List<Bar>(response.Bars);
        }

        public List<Bar> Data { get; private set; }

        public string Symbol { get; private set; }
    }
}
