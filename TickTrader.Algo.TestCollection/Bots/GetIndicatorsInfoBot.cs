using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;
using TickTrader.Algo.Api.Indicators;
using TickTrader.Algo.Indicators.ATCFMethod.FastAdaptiveTrendLine;

namespace TickTrader.Algo.TestCollection.Bots
{
    enum IndicatorsGroup
    {
        ATCFMethod,
        BillWilliams,
        Oscillators,
        Other,
        Trend,
    }

    [TradeBot(DisplayName = "[T] Get Indicators Info Bot", Version = "1.0", Category = "Test Plugin Info",
    Description = "Prints info about selected group indicators to bot status window")]
    class GetIndicatorsInfoBot : TradeBotCommon
    {
        [Parameter(DisplayName = "Selected Group", DefaultValue = IndicatorsGroup.ATCFMethod)]
        public IndicatorsGroup SelectGroup { get; set; }

        private DateTime _timeOpenLastBar = DateTime.MinValue;

        #region ATCFMethod
        private IFastAdaptiveTrendLine _fatl;
        private IFastTrendLineMomentum _ftlm;
        private IFATALSignal _fatls;
        private IFTLMSTLM _ftlmstlm;
        private IPerfectCommodityChannelIndex _pcci;
        private IRangeBoundChannelIndex _rbci;
        private IReferenceFastTrendLine _rftl;
        private IReferenceSlowTrendLine _rstl;
        private ISlowAdaptiveTrendLine _satl;
        private ISlowTrendLineMomentum _stlm;
        #endregion

        protected override void Init()
        {
            var price = Bars.Close;
            var bars = Bars;

           switch (SelectGroup)
           {
                case IndicatorsGroup.ATCFMethod:          
                    _fatl = Indicators.FastAdaptiveTrendLine(price);
                    _ftlm = Indicators.FastTrendLineMomentum(price);
                    _fatls = Indicators.FATALSignal(bars);
                    _ftlmstlm = Indicators.FTLMSTLM(price);
                    _pcci = Indicators.PerfectCommodityChannelIndex(price);
                    _rbci = Indicators.RangeBoundChannelIndex(price);
                    _rftl = Indicators.ReferenceFastTrendLine(price);
                    _rstl = Indicators.ReferenceSlowTrendLine(price);
                    _satl = Indicators.SlowAdaptiveTrendLine(price);
                    _stlm = Indicators.SlowTrendLineMomentum(price);
                    break;

                default:
                    break;
           }
        }

        protected override void OnQuote(Quote quote)
        {
            if (Bars[0].OpenTime <= _timeOpenLastBar)
                return;

            _timeOpenLastBar = Bars[0].OpenTime;

            switch (SelectGroup)
            {
                case IndicatorsGroup.ATCFMethod:
                    Status.WriteLine($"Open time bar: {Bars[0].OpenTime.ToString()}");
                    Status.WriteLine($"FATL: {_fatl.Fatl[0]:F9}");
                    Status.WriteLine($"FTLM: {_ftlm.Ftlm[0]:F9}");
                    Status.WriteLine($"FATLSignal Up: {_fatls.Up[0]}");
                    Status.WriteLine($"FATLSignal Down: {_fatls.Down[0]}");
                    Status.WriteLine($"FTLMSTLM FTLM: {_ftlmstlm.Ftlm[0]:F9}");
                    Status.WriteLine($"FTLMSTLM STLM: {_ftlmstlm.Stlm[0]:F9}");
                    Status.WriteLine($"PerfectCommodityChannelIndex: {_pcci.Pcci[0]:F9}");
                    Status.WriteLine($"RangeBoundChannelIndex Rbci: {_rbci.Rbci[0]:F9}");
                    Status.WriteLine($"RangeBoundChannelIndex UpperBound: {_rbci.UpperBound[0]:F9}");
                    Status.WriteLine($"RangeBoundChannelIndex UpperBound2: {_rbci.UpperBound2[0]:F9}");
                    Status.WriteLine($"RangeBoundChannelIndex LowerBound: {_rbci.LowerBound[0]:F9}");
                    Status.WriteLine($"RangeBoundChannelIndex LowerBound2: {_rbci.LowerBound2[0]:F9}");
                    Status.WriteLine($"ReferenceFastTrendLine: {_rftl.Rftl[0]:F9}");
                    Status.WriteLine($"ReferenceSlowTrendLine: {_rstl.Rstl[0]:F9}");
                    Status.WriteLine($"SlowAdaptiveTrendLine: {_satl.Satl[0]:F9}");
                    Status.WriteLine($"SlowTrendLineMomentum: {_stlm.Stlm[0]:F9}");
                    Status.Flush();
                    break;
                default:
                    break;
            }
        }
    }
}
