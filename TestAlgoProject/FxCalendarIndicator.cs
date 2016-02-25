using SoftFx.FxCalendar.Providers;
using System;
using System.Linq;
using TickTrader.Algo.Api;

namespace TestAlgoProject
{
    [Indicator(DisplayName = "Economic Calendar", IsOverlay = true)]
    class FxCalendarIndicator : Indicator
    {
        private FxStreetCalendar streetNewsProvider;
        bool firstRun = true;

        [Parameter(DefaultValue = "EUR")]
        public string Currency1 { get; set; }

        [Parameter(DefaultValue = "USD")]
        public string Currency2 { get; set; }

        [Input]
        public DataSeries<Bar> Input { get; set; }

        [Output(DefaultColor = Colors.Azure)]
        public DataSeries Output { get; set; }

        protected override void Init()
        {
           
        }

        protected override void Calculate()
        {
            if (firstRun)
            {
                streetNewsProvider = new FxStreetCalendar();
                streetNewsProvider.Filter.IsoCurrencyCodes = new[] { Currency1, Currency2 };
                firstRun = false;
            }

            if (!(streetNewsProvider.Filter.StartDate <= Input[0].OpenTime && streetNewsProvider.Filter.EndDate >= Input[0].CloseTime))
            {
                streetNewsProvider.Filter.StartDate = Input[0].OpenTime;
                streetNewsProvider.Filter.EndDate = Input[0].CloseTime.AddDays(10);
                streetNewsProvider.Download();
            }

            var impact = 0;
            var news = streetNewsProvider.FxNews.Where(fxN => fxN.DateUtc >= Input[0].OpenTime && fxN.DateUtc <= Input[0].CloseTime);

            if (news.Count() > 0)
                impact = news.Max(fxN => (int)fxN.Impact);

            Output[0] = Input[0].Open * Math.Pow(1.01, impact);
        }
    }
}
