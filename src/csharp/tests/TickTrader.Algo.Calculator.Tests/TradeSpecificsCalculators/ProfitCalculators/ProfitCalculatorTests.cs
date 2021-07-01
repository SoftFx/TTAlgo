using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using TestEnviroment;
using TickTrader.Algo.Calculator.Tests.TradeSpecificsCalculators;
using TickTrader.Algo.Calculator.TradeSpeсificsCalculators;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Domain.CalculatorInterfaces;

namespace TickTrader.Algo.Calculator.Tests.ProfitCalculators
{
    [TestClass]
    public class ProfitCalculatorTests : CalculatorBase
    {
        private double _price, _volume;
        private OrderInfo.Types.Side _side;

        private Func<IProfitCalculateRequest, ICalculateResponse<double>> ProfitCalculator { get; set; }


        protected override ICalculateResponse<double> ActualValue => ProfitCalculator(new ProfitRequest(_price, _volume, _side));

        protected override Func<double> ExpectedValue =>
            () => (_side.IsBuy() ? MainSymbol.Bid - _price : _price - MainSymbol.Ask) * _volume * _conversionRate();


        [TestInitialize]
        public override void TestInit()
        {
            base.TestInit();

            _price = 1;
            _volume = 1;
            _side = OrderInfo.Types.Side.Buy;
        }


        [TestMethod]
        public void EURUSD_USD_Directly_Buy()
        {
            X = "EUR";
            Y = Z = "USD";

            _price = 1.1;
            _volume = 2.0;
            _side = OrderInfo.Types.Side.Buy;

            InitEnviromentState(X + Y);

            ProfitCalculator = OrderCalculator.Profit.Calculate;
            IsSuccessfulTest();
        }

        [TestMethod]
        public void EURUSD_USD_Directly_Sell()
        {
            X = "EUR";
            Y = Z = "USD";

            _price = 0.1;
            _volume = 0.02;
            _side = OrderInfo.Types.Side.Sell;

            InitEnviromentState(X + Y);

            ProfitCalculator = OrderCalculator.Profit.Calculate;
            IsSuccessfulTest();
        }

        [TestMethod]
        public void EURUSD_EUR_Directly_Positive_Buy()
        {
            X = Z = "EUR";
            Y = "USD";

            _price = 0.1; // Bid - price >= 0
            _volume = 23.0;
            _side = OrderInfo.Types.Side.Buy;
            _conversionRate = () => 1.0 / MainSymbol.Ask;

            InitEnviromentState(X + Y);

            ProfitCalculator = OrderCalculator.Profit.Calculate;
            IsSuccessfulTest();
        }

        [TestMethod]
        public void EURUSD_EUR_Directly_Negative_Buy()
        {
            X = Z = "EUR";
            Y = "USD";

            _price = 6.77; // Bid - price < 0
            _volume = 23.0;
            _side = OrderInfo.Types.Side.Buy;
            _conversionRate = () => 1.0 / MainSymbol.Bid;

            InitEnviromentState(X + Y);

            ProfitCalculator = OrderCalculator.Profit.Calculate;
            IsSuccessfulTest();
        }

        [TestMethod]
        public void EURUSD_EUR_Directly_Positive_Sell()
        {
            X = Z = "EUR";
            Y = "USD";

            _price = 8.66; // price - Ask >= 0
            _volume = 23.0;
            _side = OrderInfo.Types.Side.Sell;
            _conversionRate = () => 1.0 / MainSymbol.Ask;

            InitEnviromentState(X + Y);

            ProfitCalculator = OrderCalculator.Profit.Calculate;
            IsSuccessfulTest();
        }

        [TestMethod]
        public void EURUSD_EUR_Directly_Negative_Sell()
        {
            X = Z = "EUR";
            Y = "USD";

            _price = 0.03; // price - Ask < 0
            _volume = 23.9;
            _side = OrderInfo.Types.Side.Sell;
            _conversionRate = () => 1.0 / MainSymbol.Bid;

            InitEnviromentState(X + Y);

            ProfitCalculator = OrderCalculator.Profit.Calculate;
            IsSuccessfulTest();
        }

        [TestMethod]
        public void EURUSD_OffQuotes()
        {
            X = Z = "EUR";
            Y = "USD";

            InitEnviromentState(X + Y);

            ProfitCalculator = OrderCalculator.Profit.Calculate;

            MainSymbol.BuildNullQuote();
            IsFailedTestOffQuotes();
        }

        [TestMethod]
        public void EURUSD_EUR_Directly_Negative_Sell_Multiple_Update()
        {
            X = Z = "EUR";
            Y = "USD";

            _price = 0.03; // price - Ask < 0
            _volume = 23.9;
            _side = OrderInfo.Types.Side.Sell;
            _conversionRate = () => 1.0 / MainSymbol.Bid;

            InitEnviromentState(X + Y);

            ProfitCalculator = OrderCalculator.Profit.Calculate;
            IsSuccessfulTest();

            MainSymbol.BuildNewQuote();
            IsSuccessfulTest();

            MainSymbol.BuildNewQuote();
            IsSuccessfulTest();
        }

        [TestMethod]
        public void EURUSD_EUR_Directly_Negative_Sell_Multiple_Update_OffQuotes()
        {
            X = Z = "EUR";
            Y = "USD";

            _price = 0.03; // price - Ask < 0
            _volume = 23.9;
            _side = OrderInfo.Types.Side.Sell;
            _conversionRate = () => 1.0 / MainSymbol.Bid;

            InitEnviromentState(X + Y);

            ProfitCalculator = OrderCalculator.Profit.Calculate;
            IsSuccessfulTest();

            MainSymbol.BuildOneSideAskQuote();
            IsFailedTestOffQuotes();

            MainSymbol.BuildNewQuote();
            IsSuccessfulTest();

            MainSymbol.BuildOneSideBidQuote();
            IsFailedTestOffQuotes();
        }

        [TestMethod]
        public void EURAUD_BTC_Cross_USD_OffCrossQuotes()
        {
            X = "EUR";
            Y = "AUD";
            Z = "BTC";
            C = "USD";

            _price = 0.231; // Bid - price >= 0
            _volume = 5.421;
            _side = OrderInfo.Types.Side.Buy;
            _conversionRate = () => Symbol[Y + C].Bid / Symbol[Z + C].Ask;

            InitEnviromentState(X + Y, Y + C, Z + C);

            ProfitCalculator = OrderCalculator.Profit.Calculate;

            Symbol[Y + C].BuildNullQuote();
            IsFailedTestOffCrossQuotes();
        }

        [TestMethod]
        public void EURAUD_BTC_Cross_USD_Positive_Buy()
        {
            X = "EUR";
            Y = "AUD";
            Z = "BTC";
            C = "USD";

            _price = 0.231; // Bid - price >= 0
            _volume = 5.421;
            _side = OrderInfo.Types.Side.Buy;
            _conversionRate = () => Symbol[Y + C].Bid / Symbol[Z + C].Ask;

            InitEnviromentState(X + Y, Y + C, Z + C);

            ProfitCalculator = OrderCalculator.Profit.Calculate;
            IsSuccessfulTest();
        }

        [TestMethod]
        public void EURAUD_BTC_Cross_USD_Positive_Buy_Multiple_Update()
        {
            X = "EUR";
            Y = "AUD";
            Z = "BTC";
            C = "USD";

            _price = 0.231; // Bid - price >= 0
            _volume = 5.421;
            _side = OrderInfo.Types.Side.Buy;
            _conversionRate = () => Symbol[Y + C].Bid / Symbol[Z + C].Ask;

            InitEnviromentState(X + Y, Y + C, Z + C);

            ProfitCalculator = OrderCalculator.Profit.Calculate;
            IsSuccessfulTest();

            Symbol[Y + C].BuildNewQuote();
            IsSuccessfulTest();

            Symbol[Z + C].BuildNewQuote();
            IsSuccessfulTest();

            Symbol[X + Y].BuildNewQuote();
            IsSuccessfulTest();
        }

        [TestMethod]
        public void EURAUD_BTC_Cross_USD_Positive_Buy_Multiple_Update_OffQuotes()
        {
            X = "EUR";
            Y = "AUD";
            Z = "BTC";
            C = "USD";

            _price = 0.231; // Bid - price >= 0
            _volume = 5.421;
            _side = OrderInfo.Types.Side.Buy;
            _conversionRate = () => Symbol[Y + C].Bid / Symbol[Z + C].Ask;

            InitEnviromentState(X + Y, Y + C, Z + C);

            ProfitCalculator = OrderCalculator.Profit.Calculate;
            IsSuccessfulTest();

            Symbol[Y + C].BuildNullQuote();
            IsFailedTestOffCrossQuotes();

            Symbol[Y + C].BuildNewQuote();
            IsSuccessfulTest();

            Symbol[Z + C].BuildNullQuote();
            IsFailedTestOffCrossQuotes();

            Symbol[Z + C].BuildNewQuote();
            IsSuccessfulTest();
        }

        [TestMethod]
        public void EURAUD_BTC_Cross_USD_Negative_Buy()
        {
            X = "EUR";
            Y = "AUD";
            Z = "BTC";
            C = "USD";

            _price = 7.901; // Bid - price < 0
            _volume = 5.421;
            _side = OrderInfo.Types.Side.Buy;
            _conversionRate = () => Symbol[Y + C].Ask / Symbol[Z + C].Bid;

            InitEnviromentState(X + Y, Y + C, Z + C);

            ProfitCalculator = OrderCalculator.Profit.Calculate;
            IsSuccessfulTest();
        }

        [TestMethod]
        public void EURAUD_BTC_Cross_USD_Negative_Buy_Multiple_Update()
        {
            X = "EUR";
            Y = "AUD";
            Z = "BTC";
            C = "USD";

            _price = 7.901; // Bid - price < 0
            _volume = 5.421;
            _side = OrderInfo.Types.Side.Buy;
            _conversionRate = () => Symbol[Y + C].Ask / Symbol[Z + C].Bid;

            InitEnviromentState(X + Y, Y + C, Z + C);

            ProfitCalculator = OrderCalculator.Profit.Calculate;
            IsSuccessfulTest();

            Symbol[Y + C].BuildNewQuote();
            IsSuccessfulTest();

            Symbol[Z + C].BuildNewQuote();
            IsSuccessfulTest();

            Symbol[X + Y].BuildNewQuote();
            IsSuccessfulTest();
        }

        [TestMethod]
        public void EURAUD_BTC_Cross_USD_Negative_Buy_Multiple_Update_OffQuotes()
        {
            X = "EUR";
            Y = "AUD";
            Z = "BTC";
            C = "USD";

            _price = 7.901; // Bid - price < 0
            _volume = 5.421;
            _side = OrderInfo.Types.Side.Buy;
            _conversionRate = () => Symbol[Y + C].Ask / Symbol[Z + C].Bid;

            InitEnviromentState(X + Y, Y + C, Z + C);

            ProfitCalculator = OrderCalculator.Profit.Calculate;
            IsSuccessfulTest();

            Symbol[Y + C].BuildNullQuote();
            IsFailedTestOffCrossQuotes();

            Symbol[Y + C].BuildNewQuote();
            IsSuccessfulTest();

            Symbol[Z + C].BuildNullQuote();
            IsFailedTestOffCrossQuotes();

            Symbol[Z + C].BuildNewQuote();
            IsSuccessfulTest();
        }

        [TestMethod]
        public void EURAUD_BTC_Cross_USD_Positive_Sell()
        {
            X = "EUR";
            Y = "AUD";
            Z = "BTC";
            C = "USD";

            _price = 13.11; // price - Ask >= 0
            _volume = 5.421;
            _side = OrderInfo.Types.Side.Sell;
            _conversionRate = () => Symbol[Y + C].Bid / Symbol[Z + C].Ask;

            InitEnviromentState(X + Y, Y + C, Z + C);

            ProfitCalculator = OrderCalculator.Profit.Calculate;
            IsSuccessfulTest();
        }

        [TestMethod]
        public void EURAUD_BTC_Cross_USD_Positive_Sell_Multiple_Update()
        {
            X = "EUR";
            Y = "AUD";
            Z = "BTC";
            C = "USD";

            _price = 13.11; // price - Ask >= 0
            _volume = 5.421;
            _side = OrderInfo.Types.Side.Sell;
            _conversionRate = () => Symbol[Y + C].Bid / Symbol[Z + C].Ask;

            InitEnviromentState(X + Y, Y + C, Z + C);

            ProfitCalculator = OrderCalculator.Profit.Calculate;
            IsSuccessfulTest();

            Symbol[Y + C].BuildNewQuote();
            IsSuccessfulTest();

            Symbol[Z + C].BuildNewQuote();
            IsSuccessfulTest();

            Symbol[X + Y].BuildNewQuote();
            IsSuccessfulTest();
        }

        [TestMethod]
        public void EURAUD_BTC_Cross_USD_Positive_Sell_Multiple_Update_OffQuotes()
        {
            X = "EUR";
            Y = "AUD";
            Z = "BTC";
            C = "USD";

            _price = 13.11; // price - Ask >= 0
            _volume = 5.421;
            _side = OrderInfo.Types.Side.Sell;
            _conversionRate = () => Symbol[Y + C].Bid / Symbol[Z + C].Ask;

            InitEnviromentState(X + Y, Y + C, Z + C);

            ProfitCalculator = OrderCalculator.Profit.Calculate;
            IsSuccessfulTest();

            Symbol[Y + C].BuildNullQuote();
            IsFailedTestOffCrossQuotes();

            Symbol[Y + C].BuildNewQuote();
            IsSuccessfulTest();

            Symbol[Z + C].BuildNullQuote();
            IsFailedTestOffCrossQuotes();

            Symbol[Z + C].BuildNewQuote();
            IsSuccessfulTest();
        }

        [TestMethod]
        public void EURAUD_BTC_Cross_USD_Negative_Sell()
        {
            X = "EUR";
            Y = "AUD";
            Z = "BTC";
            C = "USD";

            _price = 0.03; // price - Ask < 0
            _volume = 5.421;
            _side = OrderInfo.Types.Side.Sell;
            _conversionRate = () => Symbol[Y + C].Ask / Symbol[Z + C].Bid;

            InitEnviromentState(X + Y, Y + C, Z + C);

            ProfitCalculator = OrderCalculator.Profit.Calculate;
            IsSuccessfulTest();
        }

        [TestMethod]
        public void EURAUD_BTC_Cross_USD_Negative_Sell_Multiple_Update()
        {
            X = "EUR";
            Y = "AUD";
            Z = "BTC";
            C = "USD";

            _price = 0.03; // price - Ask < 0
            _volume = 5.421;
            _side = OrderInfo.Types.Side.Sell;
            _conversionRate = () => Symbol[Y + C].Ask / Symbol[Z + C].Bid;

            InitEnviromentState(X + Y, Y + C, Z + C);

            ProfitCalculator = OrderCalculator.Profit.Calculate;
            IsSuccessfulTest();

            Symbol[Y + C].BuildNewQuote();
            IsSuccessfulTest();

            Symbol[Z + C].BuildNewQuote();
            IsSuccessfulTest();

            Symbol[X + Y].BuildNewQuote();
            IsSuccessfulTest();
        }

        [TestMethod]
        public void EURAUD_BTC_Cross_USD_Negative_Sell_Multiple_Update_OffQuotes()
        {
            X = "EUR";
            Y = "AUD";
            Z = "BTC";
            C = "USD";

            _price = 0.03; // price - Ask < 0
            _volume = 5.421;
            _side = OrderInfo.Types.Side.Sell;
            _conversionRate = () => Symbol[Y + C].Ask / Symbol[Z + C].Bid;

            InitEnviromentState(X + Y, Y + C, Z + C);

            ProfitCalculator = OrderCalculator.Profit.Calculate;
            IsSuccessfulTest();

            Symbol[Y + C].BuildNullQuote();
            IsFailedTestOffCrossQuotes();

            Symbol[Y + C].BuildNewQuote();
            IsSuccessfulTest();

            Symbol[Z + C].BuildNullQuote();
            IsFailedTestOffCrossQuotes();

            Symbol[Z + C].BuildNewQuote();
            IsSuccessfulTest();
        }
    }
}
