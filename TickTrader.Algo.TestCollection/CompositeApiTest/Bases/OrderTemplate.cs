using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TickTrader.Algo.Api;
using TickTrader.Algo.Api.Math;

namespace TickTrader.Algo.TestCollection.CompositeApiTest
{
    internal class OrderTemplate : TestParamsSet
    {
        private DateTime? _expiration;


        public Order RealOrder { get; private set; }

        public bool CanCloseOrder => RealOrder.Type == OrderType.Position || RealOrder.Type == OrderType.Market;

        public bool IsNull => Volume.E(0.0);


        public TaskCompletionSource<bool> Opened { get; private set; }

        public TaskCompletionSource<bool> OpenedGrossPosition { get; private set; }

        public TaskCompletionSource<bool> Filled { get; private set; }

        public TaskCompletionSource<bool> FinalExecution => IsGrossAcc ? OpenedGrossPosition : Filled;


        public double SlippagePrecision { get; }

        public OrderType InitType { get; protected set; }

        public string Id { get; set; } = string.Empty;

        public double? Price { get; set; }

        public double? StopPrice { get; set; }

        public double? MaxVisibleVolume { get; set; }

        public double? SL { get; set; }

        public double? TP { get; set; }

        public double ReqVolume { get; set; }

        public double RemVolume { get; set; }

        public double Volume { get; set; }

        public double? Slippage { get; set; }

        public string Comment { get; set; }

        public string Tag { get; set; }

        public DateTime? Expiration
        {
            get => _expiration;

            set
            {
                _expiration = value?.AddMilliseconds(-value.Value.Millisecond); //TTS reset milliseconds
            }
        }

        public string OcoRelatedOrderId { get; set; }


        public List<OrderTemplate> FilledParts { get; private set; }


        public OrderTemplate() { }

        public OrderTemplate(TestParamsSet test, double volume) : base(test.Type, test.Side)
        {
            SetVolume(volume);
            Opened = new TaskCompletionSource<bool>();
            Filled = new TaskCompletionSource<bool>();
            OpenedGrossPosition = new TaskCompletionSource<bool>();
            FilledParts = new List<OrderTemplate>();

            Options = test.Options;

            InitType = Type;
            SlippagePrecision = Math.Pow(10, Math.Max(Symbol.Digits, 4));
        }

        private void SetVolume(double volume)
        {
            ReqVolume = volume;
            RemVolume = volume;
            Volume = volume;
        }

        public OrderTemplate ForPending(int coef = 3)
        {
            Price = CalculatePrice(coef);
            StopPrice = CalculatePrice(-coef);

            return this;
        }

        public OrderTemplate ForExecuting(int coef = 3)
        {
            Price = CalculatePrice(-coef);
            StopPrice = CalculatePrice(coef);

            return this;
        }

        public OrderTemplate ForGrossPositionPending(int coef = 3, string comment = "")
        {
            TP = CalculatePrice(-coef);
            SL = CalculatePrice(coef);
            Comment = comment;

            return ForExecuting(coef);
        }

        internal double? CalculatePrice(int coef)
        {
            var delta = coef * PriceDelta * Symbol.Point;

            return Side.IsBuy() ? Symbol.Ask - delta : Symbol.Bid + delta;
        }

        internal OpenOrderRequest GetOpenRequest()
        {
            return OpenOrderRequest.Template.Create()
                   .WithParams(Symbol.Name, Side, Type, Volume, Price, StopPrice)
                   .WithMaxVisibleVolume(MaxVisibleVolume)
                   .WithSlippage(GetSlippageInPercent())
                   .WithExpiration(Expiration)
                   .WithComment(Comment)
                   .WithOptions(Options)
                   .WithTakeProfit(TP)
                   .WithStopLoss(SL)
                   .WithTag(Tag)
                   .MakeRequest();
        }

        internal ModifyOrderRequest GetModifyRequest()
        {
            return ModifyOrderRequest.Template.Create()
                   .WithParams(Id, Price)
                   .WithMaxVisibleVolume(MaxVisibleVolume)
                   .WithSlippage(GetSlippageInPercent())
                   .WithExpiration(Expiration)
                   .WithStopPrice(StopPrice)
                   .WithComment(Comment)
                   .WithOptions(Options)
                   .WithVolume(Volume)
                   .WithTakeProfit(TP)
                   .WithStopLoss(SL)
                   .WithTag(Tag)
                   .MakeRequest();
        }

        internal CloseOrderRequest GetCloseRequest(double? volume = null)
        {
            return CloseOrderRequest.Template.Create()
                   .WithParams(Id, volume)
                   .WithSlippage(GetSlippageInPercent())
                   .MakeRequest();
        }

        internal OrderTemplate ToOpen(string orderId)
        {
            Id = orderId;
            RealOrder = Orders[Id];
            Opened.SetResult(true);

            return this;
        }

        internal OrderTemplate ToActivate()
        {
            Opened = new TaskCompletionSource<bool>();
            InitType = OrderType.StopLimit;
            Type = OrderType.Limit;

            return this;
        }

        internal OrderTemplate ToFilled(double filledVolume)
        {
            RemVolume -= filledVolume;

            if (RemVolume.Gt(0.0))
            {
                var filledPart = Copy();
                filledPart.Volume = filledVolume;
                filledPart.Filled.SetResult(true);

                FilledParts.Add(filledPart);

                return filledPart;
            }

            Filled.SetResult(true);
            return this;
        }

        internal OrderTemplate ToGrossPosition()
        {
            Opened = new TaskCompletionSource<bool>();
            InitType = Type;
            Type = OrderType.Position;
            OpenedGrossPosition.SetResult(true);

            return this;
        }

        public double? GetSlippageInPercent()
        {
            if (Slippage != null)
                Slippage = SlippageConverters.SlippagePipsToFractions(Slippage.Value, (IsSupportedStopPrice ? StopPrice : Price).Value, Symbol);

            return Slippage;
        }

        protected double CeilSlippage(double slippage) => Math.Round(Math.Ceiling(slippage * SlippagePrecision) / SlippagePrecision, Symbol.Digits);

        protected double GetMaxSlippage() => SlippageConverters.SlippagePipsToFractions(Symbol.Slippage, (IsSupportedStopPrice ? StopPrice : Price).Value, Symbol);

        protected void CheckSlippage(double serverSlippage, Action<double, double> comparer)
        {
            var calcSlippage = GetMaxSlippage();
            var expectedSlippage = CeilSlippage(Slippage == null || Slippage.Value > calcSlippage ? calcSlippage : Slippage.Value);

            comparer(expectedSlippage, CeilSlippage(serverSlippage));
        }

        //add deep copy trigger property later
        public OrderTemplate Copy()
        {
            var copy = (OrderTemplate)MemberwiseClone();

            copy.Opened = new TaskCompletionSource<bool>();
            copy.Filled = new TaskCompletionSource<bool>();
            copy.FilledParts = new List<OrderTemplate>();
            copy.OpenedGrossPosition = new TaskCompletionSource<bool>();

            return copy;
        }

        public OrderTemplate InversedCopy(double? newVolume = null)
        {
            var copy = Copy();

            copy.Side = Side == OrderSide.Buy ? OrderSide.Sell : OrderSide.Buy;
            copy.SetVolume(newVolume ?? Volume);

            return copy;
        }

        public static OrderTemplate operator +(OrderTemplate first, OrderTemplate second)
        {
            var resultCopy = first.Volume > second.Volume ? first.Copy() : second.Copy();
            var resultVolume = first.Side == second.Side ?
                               first.Volume + second.Volume :
                               Math.Abs(first.Volume - second.Volume);

            resultCopy.SetVolume(resultVolume);

            return resultCopy;
        }
    }
}
