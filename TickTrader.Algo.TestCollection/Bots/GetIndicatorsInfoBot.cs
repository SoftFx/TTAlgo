using System;
using TickTrader.Algo.Api;
using TickTrader.Algo.Api.Indicators;

namespace TickTrader.Algo.TestCollection.Bots
{
    enum IndicatorsGroup
    {
        ATCFMethod,
        BillWilliams,
        Oscillators,
        Other,
        Trend,
        Volumes
    }

    [TradeBot(DisplayName = "[T] Get Indicators Info Bot", Version = "1.0", Category = "Test Plugin Info",
    Description = "Prints info about selected group indicators to bot status window")]
    class GetIndicatorsInfoBot : TradeBotCommon
    {
        [Parameter(DisplayName = "Selected Group", DefaultValue = IndicatorsGroup.ATCFMethod)]
        public IndicatorsGroup SelectGroup { get; set; }

        //ATCF method
        private IFastAdaptiveTrendLine _fatl;
        private IFastTrendLineMomentum _ftlm;
        private IFATLSignal _fatls;
        private IFTLMSTLM _ftlmstlm;
        private IPerfectCommodityChannelIndex _pcci;
        private IRangeBoundChannelIndex _rbci;
        private IReferenceFastTrendLine _rftl;
        private IReferenceSlowTrendLine _rstl;
        private ISlowAdaptiveTrendLine _satl;
        private ISlowTrendLineMomentum _stlm;


        //BillWilliams
        private IAcceleratorOscillator _acceleratorOscillator;
        private IAlligator _alligator;
        private IAwesomeOscillator _awesomeOscillator;
        private IFractals _fractals;
        private IGatorOscillator _gatorOscillator;
        private IMarketFacilitationIndex _marketFacilitationIndex;


        //Oscillators
        private IAverageTrueRange _avr;
        private IBearsPower _bearsPower;
        private IBullsPower _bullsPower;
        private ICommodityChannelIndex _cci;
        private IDeMarker _deMarker;
        private IForceIndex _fi;
        private IMacd _macd;
        private IMomentum _momentum;
        private IMovingAverageOscillator _mao;
        private IRelativeStrenghtIndex _rsi;
        private IRelativeVigorIndex _rvi;
        private IStochasticOscillator _so;
        private IWilliamsPercentRange _wpr;


        //Other
        private IHeikenAshi _ha;
        private IZigZag _zz;


        //Trend
        private IAverageDirectionalMovementIndex _admi;
        private IBoolingerBands _bb;
        private IEnvelopes _envelopes;
        private IIchimokuKinkoHyo _ikh;
        private IMovingAverage _ma;
        private IParabolicSar _ps;
        private IStandardDeviation _sd;


        //Volumes
        private IAccumulationDistribution _ad;
        private IMoneyFlowIndex _mfi;
        private IOnBalanceVolume _obv;
        private IVolumes _volumes;


        protected override void Init()
        {
            var price = Bars.Close;
            var bars = Bars;

            switch (SelectGroup)
            {
                case IndicatorsGroup.ATCFMethod:

                    _fatl = Indicators.FastAdaptiveTrendLine(price);
                    _ftlm = Indicators.FastTrendLineMomentum(price);
                    _fatls = Indicators.FATLSignal(bars);
                    _ftlmstlm = Indicators.FTLMSTLM(price);
                    _pcci = Indicators.PerfectCommodityChannelIndex(price);
                    _rbci = Indicators.RangeBoundChannelIndex(price);
                    _rftl = Indicators.ReferenceFastTrendLine(price);
                    _rstl = Indicators.ReferenceSlowTrendLine(price);
                    _satl = Indicators.SlowAdaptiveTrendLine(price);
                    _stlm = Indicators.SlowTrendLineMomentum(price);

                    break;

                case IndicatorsGroup.BillWilliams:

                    _acceleratorOscillator = Indicators.AcceleratorOscillator(bars);
                    _alligator = Indicators.Alligator(price);
                    _awesomeOscillator = Indicators.AwesomeOscillator(bars);
                    _fractals = Indicators.Fractals(bars);
                    _gatorOscillator = Indicators.GatorOscillator(price);
                    _marketFacilitationIndex = Indicators.MarketFacilitationIndex(bars);

                    break;

                case IndicatorsGroup.Oscillators:

                    _avr = Indicators.AverageTrueRange(bars);
                    _bearsPower = Indicators.BearsPower(bars);
                    _bullsPower = Indicators.BullsPower(bars);
                    _cci = Indicators.CommodityChannelIndex(price);
                    _deMarker = Indicators.DeMarker(bars);
                    _fi = Indicators.ForceIndex(bars); 
                    _macd = Indicators.MACD(price);
                    _momentum = Indicators.Momentum(price);
                    _mao = Indicators.MovingAverageOscillator(price);
                    _rsi = Indicators.RelativeStrenghtIndex(price);
                    _rvi = Indicators.RelativeVigorIndex(bars);
                    _so = Indicators.StochasticOscillator(bars);
                    _wpr = Indicators.WilliamsPercentRange(bars);

                    break;

                case IndicatorsGroup.Other:

                    _ha = Indicators.HeikenAshi(bars);
                    _zz = Indicators.ZigZag(bars);

                    break;

                case IndicatorsGroup.Trend:

                    _admi = Indicators.AverageDirectionalMovementIndex(bars);
                    _bb = Indicators.BoolingerBands(price);
                    _envelopes = Indicators.Envelopes(price);
                    _ikh = Indicators.IchimokuKinHyo(bars);
                    _ma = Indicators.MovingAverage(price);
                    _ps = Indicators.ParabolicSar(bars);
                    _sd = Indicators.StandardDeviation(price);

                    break;

                case IndicatorsGroup.Volumes:

                    _ad = Indicators.AccumulationDistribution(bars);
                    _mfi = Indicators.MoneyFlowIndex(bars);
                    _obv = Indicators.OnBalanceVolume(bars);
                    _volumes = Indicators.Volumes(bars);

                    break;

                default:
                
                    break;
            }

            Status.WriteLine("Bot init");
            Status.Flush();
        }

        protected override void OnQuote(Quote quote)
        { 
            Status.WriteLine($"Open time bar: {Bars[0].OpenTime.ToString()}\n");

            switch (SelectGroup)
            {
                case IndicatorsGroup.ATCFMethod:

                    Status.WriteLine($"FATL: {_fatl.Fatl[0]:F9}");
                    Status.WriteLine($"FTLM: {_ftlm.Ftlm[0]:F9}\n");

                    Status.WriteLine($"FATLSignal Up: {_fatls.Up[0]}");
                    Status.WriteLine($"FATLSignal Down: {_fatls.Down[0]}\n");

                    Status.WriteLine($"FTLMSTLM FTLM: {_ftlmstlm.Ftlm[0]:F9}");
                    Status.WriteLine($"FTLMSTLM STLM: {_ftlmstlm.Stlm[0]:F9}\n");

                    Status.WriteLine($"PerfectCommodityChannelIndex: {_pcci.Pcci[0]:F9}\n");

                    Status.WriteLine($"RangeBoundChannelIndex Rbci: {_rbci.Rbci[0]:F9}");
                    Status.WriteLine($"RangeBoundChannelIndex UpperBound: {_rbci.UpperBound[0]:F9}");
                    Status.WriteLine($"RangeBoundChannelIndex UpperBound2: {_rbci.UpperBound2[0]:F9}");
                    Status.WriteLine($"RangeBoundChannelIndex LowerBound: {_rbci.LowerBound[0]:F9}");
                    Status.WriteLine($"RangeBoundChannelIndex LowerBound2: {_rbci.LowerBound2[0]:F9}\n");

                    Status.WriteLine($"ReferenceFastTrendLine: {_rftl.Rftl[0]:F9}\n");
                    Status.WriteLine($"ReferenceSlowTrendLine: {_rstl.Rstl[0]:F9}\n");

                    Status.WriteLine($"SlowAdaptiveTrendLine: {_satl.Satl[0]:F9}");
                    Status.WriteLine($"SlowTrendLineMomentum: {_stlm.Stlm[0]:F9}\n");

                    break;

                case IndicatorsGroup.BillWilliams:

                    Status.WriteLine($"AcceleratorOscillator valueUp: {_acceleratorOscillator.ValueUp[0]:F9}");
                    Status.WriteLine($"AcceleratorOscillator valueDown: {_acceleratorOscillator.ValueDown[0]:F9}\n");

                    Status.WriteLine($"Alligator Jaws: {_alligator.Jaws[0]:F9}");
                    Status.WriteLine($"Alligator Teeth: {_alligator.Teeth[0]:F9}");
                    Status.WriteLine($"Alligator Lips: {_alligator.Lips[0]:F9}\n");

                    Status.WriteLine($"AwesomeOscillator valueUp: {_awesomeOscillator.ValueUp[0]:F9}");
                    Status.WriteLine($"AwesomeOscillator valueDown: {_awesomeOscillator.ValueDown[0]:F9}\n");

                    Status.WriteLine($"Fractals FractalsUp: {_fractals.FractalsUp[0].Y:F9}");
                    Status.WriteLine($"Fractals FractalsDown: {_fractals.FractalsDown[0].Y:F9}\n");

                    Status.WriteLine($"GatorOscillator TeethLipsUp: {_gatorOscillator.TeethLipsUp[0]:F9}");
                    Status.WriteLine($"GatorOscillator TeethLipsDown: {_gatorOscillator.TeethLipsDown[0]:F9}");
                    Status.WriteLine($"GatorOscillator JawsTeethUp: {_gatorOscillator.JawsTeethUp[0]:F9}");
                    Status.WriteLine($"GatorOscillator JawsTeethDown: {_gatorOscillator.JawsTeethDown[0]:F9}\n");

                    Status.WriteLine($"MarketFacilitationIndex MfiUpVolumeUp: {_marketFacilitationIndex.MfiUpVolumeUp[0]:F9}");
                    Status.WriteLine($"MarketFacilitationIndex MfiUpVolumeDown: {_marketFacilitationIndex.MfiUpVolumeDown[0]:F9}");
                    Status.WriteLine($"MarketFacilitationIndex MfiDownVolumeUp: {_marketFacilitationIndex.MfiDownVolumeUp[0]:F9}");
                    Status.WriteLine($"MarketFacilitationIndex MfiDownVolumeDown: {_marketFacilitationIndex.MfiDownVolumeDown[0]:F9}\n");

                    break;

                case IndicatorsGroup.Oscillators:

                    Status.WriteLine($"AverageTrueRanger: {_avr.Atr[0]:F9}\n");

                    Status.WriteLine($"BearsPower: {_bearsPower.Bears[0]:F9}\n");

                    Status.WriteLine($"BullsPower: {_bullsPower.Bulls[0]:F9}\n");

                    Status.WriteLine($"CommodityChannelIndex: {_cci.Cci[0]:F9}\n");

                    Status.WriteLine($"DeMarker: {_deMarker.DeMark[0]:F9}\n");

                    Status.WriteLine($"ForceIndex: {_fi.Force[0]:F9}\n");

                    Status.WriteLine($"MACD series: {_macd.MacdSeries[0]:F9}");
                    Status.WriteLine($"MACD signals: {_macd.Signal[0]:F9}\n");

                    Status.WriteLine($"Momentum: {_momentum.Moment[0]:F9}\n");

                    Status.WriteLine($"MA oscillator: {_mao.OsMa[0]:F9}\n");

                    Status.WriteLine($"Relative Strength Index: {_rsi.Rsi[0]:F9}\n");

                    Status.WriteLine($"Relative Vigor Index RviAverage: {_rvi.RviAverage[0]:F9}");
                    Status.WriteLine($"Relative Vigor Index Signal: {_rvi.Signal[0]:F9}\n");

                    Status.WriteLine($"StochasticOscillator Stoch: {_so.Stoch[0]:F9}");
                    Status.WriteLine($"StochasticOscillator Signal: {_so.Signal[0]:F9}\n");

                    Status.WriteLine($"Williams Percent Range: {_wpr.Wpr[0]:F9}\n");

                    break;

                case IndicatorsGroup.Other:

                    Status.WriteLine($"HeikenAshi Low/High: {_ha.HaLowHigh[0]:F9}");
                    Status.WriteLine($"HeikenAshi High/Low: {_ha.HaHighLow[0]:F9}");
                    Status.WriteLine($"HeikenAshi Open: {_ha.HaOpen[0]:F9}");
                    Status.WriteLine($"HeikenAshi Close: {_ha.HaClose[0]:F9}\n");

                    Status.WriteLine($"ZigZag: {_zz.Zigzag[0]:F9}");
                    Status.WriteLine($"ZigZag line: {_zz.ZigzagLine[0]:F9}\n");

                    break;

                case IndicatorsGroup.Trend:

                    Status.WriteLine($"AverageDirectionalMovementIndex ADX: {_admi.Adx[0]:F9}");
                    Status.WriteLine($"AverageDirectionalMovementIndex +DMI: {_admi.PlusDmi[0]:F9}");
                    Status.WriteLine($"AverageDirectionalMovementIndex -DMI: {_admi.MinusDmi[0]:F9}\n");

                    Status.WriteLine($"BollingerBands Middle Line: {_bb.MiddleLine[0]:F9}");
                    Status.WriteLine($"BollingerBands Top Line: {_bb.TopLine[0]:F9}");
                    Status.WriteLine($"BollingerBands Botton Line: {_bb.BottomLine[0]:F9}\n");

                    Status.WriteLine($"Envelopes Top Line: {_envelopes.TopLine[0]:F9}");
                    Status.WriteLine($"Envelopes Botton Line: {_envelopes.BottomLine[0]:F9}\n");

                    Status.WriteLine($"IchimokuKinkoHyo Tenkan-sen: {_ikh.Tenkan[0]:F9}");
                    Status.WriteLine($"IchimokuKinkoHyo Kijun-sen: {_ikh.Kijun[0]:F9}");
                    Status.WriteLine($"IchimokuKinkoHyo Senkou Span A: {_ikh.SenkouA[0]:F9}");
                    Status.WriteLine($"IchimokuKinkoHyo Senkou Span B: {_ikh.SenkouB[0]:F9}");
                    Status.WriteLine($"IchimokuKinkoHyo Chikou Span: {_ikh.Chikou[0]:F9}\n");

                    Status.WriteLine($"Moving Average: {_ma.Average[0]:F9}\n");

                    Status.WriteLine($"Parabolic SAR: {_ps.Sar[0]:F9}\n");

                    Status.WriteLine($"StandardDeviation: {_sd.StdDev[0]:F9}\n");

                    break;

                case IndicatorsGroup.Volumes:

                    Status.WriteLine($"AccumulationDistribution: {_ad.Ad[0]:F9}\n");

                    Status.WriteLine($"MoneyFlowIndex: {_mfi.Mfi[0]:F9}\n");

                    Status.WriteLine($"OnBalanceVolume: {_obv.Obv[0]:F9}\n");

                    Status.WriteLine($"Volume Up: {_volumes.ValueUp[0]:F9}");
                    Status.WriteLine($"Volume Down: {_volumes.ValueDown[0]:F9}");

                    break;

                default:
                    break;
            }

            Status.Flush();
        }
    }
}
