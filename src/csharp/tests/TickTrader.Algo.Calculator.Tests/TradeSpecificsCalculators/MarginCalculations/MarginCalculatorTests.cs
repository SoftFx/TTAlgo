using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using TestEnviroment;
using TickTrader.Algo.Calculator.Tests.TradeSpecificsCalculators;
using TickTrader.Algo.Calculator.TradeSpeсificsCalculators;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Domain.CalculatorInterfaces;

namespace TickTrader.Algo.Calculator.Tests.MarginCalculations
{
    [TestClass]
    public class MarginCalculatorTests : CalculatorBase
    {
        private double? _marginFactor;
        private double _volume = 1;
        private bool _isHiddenOrder;
        private OrderInfo.Types.Type _orderType;

        private Func<IMarginCalculateRequest, ICalculateResponse<double>> MarginCalculator { get; set; }


        protected override ICalculateResponse<double> ActualValue => MarginCalculator(new MarginRequest(_volume, _orderType, _isHiddenOrder));

        protected override Func<double> ExpectedValue => () => _volume * _conversionRate() * MainSymbol.Margin.Factor / _leverage * (_marginFactor ?? 1.0);


        [TestInitialize]
        public override void TestInit()
        {
            base.TestInit();

            _volume = 1;
            _marginFactor = null;
            _isHiddenOrder = false;
            _orderType = OrderInfo.Types.Type.Market;
        }

        [TestMethod]
        public void EURUSD_EUR_Directly_Limit()
        {
            X = Z = "EUR";
            Y = "USD";

            _volume = 2.0;

            InitEnviromentState(X + Y);

            _orderType = OrderInfo.Types.Type.Limit;

            MarginCalculator = OrderCalculator.Margin.Calculate;
            IsSuccessfulTest();
        }

        [TestMethod]
        public void EURUSD_EUR_Directly_Limit_Hidden()
        {
            X = Z = "EUR";
            Y = "USD";

            _leverage = 3;
            _marginFactor = MainSymbol.Margin.HiddenLimitOrderReduction;

            InitEnviromentState(X + Y);

            _orderType = OrderInfo.Types.Type.Limit;
            _isHiddenOrder = true;

            MarginCalculator = OrderCalculator.Margin.Calculate;
            IsSuccessfulTest();
        }

        [TestMethod]
        public void EURUSD_EUR_Directly_Stop()
        {
            X = Z = "EUR";
            Y = "USD";

            _leverage = 2;
            _volume = 0.01;
            _marginFactor = MainSymbol.Margin.StopOrderReduction;

            InitEnviromentState(X + Y);

            _orderType = OrderInfo.Types.Type.Stop;
            _isHiddenOrder = true;

            MarginCalculator = OrderCalculator.Margin.Calculate;
            IsSuccessfulTest();
        }

        [TestMethod]
        public void EURUSD_EUR_OffQuotes()
        {
            X = "EUR";
            Y = Z = "USD";

            _conversionRate = () => Symbol[X + Y].Ask;

            InitEnviromentState(); // symbol X + Y doesn't load

            Symbol[X + Y].BuildNullQuote();
            MarginCalculator = OrderCalculator.Margin.Calculate;
            IsFailedTestOffQuotes();
        }

        [TestMethod]
        public void EURUSD_AUD_Directly_Limit()
        {
            X = "EUR";
            Y = "USD";
            Z = "AUD";

            _leverage = 1;
            _volume = 1;
            _conversionRate = () => Symbol[X + Z].Ask;
            _marginFactor = null;

            InitEnviromentState(X + Y, X + Z);

            _orderType = OrderInfo.Types.Type.Limit;
            _isHiddenOrder = false;

            MarginCalculator = OrderCalculator.Margin.Calculate;
            IsSuccessfulTest();
        }

        [TestMethod]
        public void EURUSD_AUD_Directly_CrossSymbolNotFound()
        {
            X = "EUR";
            Y = "USD";
            Z = "AUD";

            _volume = 1.1;
            _conversionRate = () => Symbol[X + Z].Ask;
            _marginFactor = null;

            InitEnviromentState(X + Y); // X + Z symbol doesn't load
            _orderType = OrderInfo.Types.Type.Limit;
            _isHiddenOrder = false;

            MarginCalculator = OrderCalculator.Margin.Calculate;
            IsFailedTestCrossSymbolNotFound();
        }

        [TestMethod]
        public void EURUSD_AUD_Directly_Limit_Multiple_Update()
        {
            X = "EUR";
            Y = "USD";
            Z = "AUD";

            _leverage = 1;
            _volume = 0.001;
            _conversionRate = () => Symbol[X + Z].Ask;
            _marginFactor = null;

            InitEnviromentState(X + Y, X + Z);

            _orderType = OrderInfo.Types.Type.Limit;
            _isHiddenOrder = false;

            MarginCalculator = OrderCalculator.Margin.Calculate;
            IsSuccessfulTest();

            Symbol[X + Y].BuildNewQuote();
            IsSuccessfulTest();

            Symbol[X + Z].BuildNewQuote();
            IsSuccessfulTest();
        }

        [TestMethod]
        public void EURAUD_BTC_Cross_USD_Stop()
        {
            X = "EUR";
            Y = "AUD";
            Z = "BTC";
            C = "USD";

            _leverage = 10;
            _volume = 0.3;
            _conversionRate = () => Symbol[Y + C].Ask / Symbol[Z + C].Bid * Symbol[X + Y].Ask;
            _marginFactor = MainSymbol.Margin.StopOrderReduction;

            InitEnviromentState(X + Y, Y + C, Z + C);

            _orderType = OrderInfo.Types.Type.Stop;
            _isHiddenOrder = false;

            MarginCalculator = OrderCalculator.Margin.Calculate;
            IsSuccessfulTest();
        }

        [TestMethod]
        public void EURAUD_BTC_Cross_USD_Stop_Multiple_Update()
        {
            X = "EUR";
            Y = "AUD";
            Z = "BTC";
            C = "USD";

            _leverage = 10;
            _volume = 0.3;
            _conversionRate = () => Symbol[Y + C].Ask / Symbol[Z + C].Bid * Symbol[X + Y].Ask;
            _marginFactor = MainSymbol.Margin.StopOrderReduction;

            InitEnviromentState(X + Y, Y + C, Z + C);

            _orderType = OrderInfo.Types.Type.Stop;
            _isHiddenOrder = false;

            MarginCalculator = OrderCalculator.Margin.Calculate;

            IsSuccessfulTest();

            Symbol[X + Y].BuildNewQuote();
            IsSuccessfulTest();

            Symbol[Y + C].BuildNewQuote();
            IsSuccessfulTest();

            Symbol[Z + C].BuildNewQuote();
            IsSuccessfulTest();

            Symbol[Y + C].BuildNewQuote();
            Symbol[Z + C].BuildNewQuote();
            IsSuccessfulTest();
        }

        [TestMethod]
        public void EURAUD_BTC_Cross_USD_Hiddent_Limit()
        {
            X = "EUR";
            Y = "AUD";
            Z = "BTC";
            C = "USD";

            _leverage = 10;
            _volume = 0.3;
            _conversionRate = () => Symbol[Y + C].Ask / Symbol[Z + C].Bid * Symbol[X + Y].Ask;
            _marginFactor = MainSymbol.Margin.HiddenLimitOrderReduction;

            InitEnviromentState(X + Y, Y + C, Z + C);

            _orderType = OrderInfo.Types.Type.Limit;
            _isHiddenOrder = false;

            MarginCalculator = OrderCalculator.Margin.Calculate;
            IsSuccessfulTest();
        }

        [TestMethod]
        public void EURAUD_BTC_Cross_USD_Hiddent_Limit_OffCrossQuotes()
        {
            X = "EUR";
            Y = "AUD";
            Z = "BTC";
            C = "USD";

            _leverage = 5;
            _volume = 0.3;
            _conversionRate = () => Symbol[Y + C].Ask / Symbol[Z + C].Bid * Symbol[X + Y].Ask;
            _marginFactor = MainSymbol.Margin.HiddenLimitOrderReduction;

            InitEnviromentState(X + Y, Y + C, Z + C);

            _orderType = OrderInfo.Types.Type.Limit;
            _isHiddenOrder = false;

            MarginCalculator = OrderCalculator.Margin.Calculate;

            Symbol[Z + C].BuildNullQuote();
            IsFailedTestOffCrossQuotes();
        }

        [TestMethod]
        public void EURAUD_BTC_Cross_USD_Hiddent_Limit_Multiple_Update_With_OffCrossQuotes()
        {
            X = "EUR";
            Y = "AUD";
            Z = "BTC";
            C = "USD";

            _leverage = 5;
            _volume = 0.3;
            _conversionRate = () => Symbol[Y + C].Ask / Symbol[Z + C].Bid * Symbol[X + Y].Ask;
            _marginFactor = MainSymbol.Margin.HiddenLimitOrderReduction;

            InitEnviromentState(X + Y, Y + C, Z + C);

            _orderType = OrderInfo.Types.Type.Limit;
            _isHiddenOrder = false;

            MarginCalculator = OrderCalculator.Margin.Calculate;

            IsSuccessfulTest();

            Symbol[Z + C].BuildNullQuote();
            IsFailedTestOffCrossQuotes();

            Symbol[Z + C].BuildOneSideBidQuote();
            IsSuccessfulTest();

            Symbol[X + Y].BuildNullQuote();
            IsFailedTestOffCrossQuotes();

            Symbol[X + Y].BuildNewQuote();
            IsSuccessfulTest();
        }
    }
}
