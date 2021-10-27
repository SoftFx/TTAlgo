using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;
using TickTrader.Algo.Api.Math;

namespace TickTrader.Algo.TestCollection.CompositeApiTest
{
    public class OrderTemplate : TestParamsSet
    {
        private string _id = string.Empty;
        private DateTime? _expiration;


        public Order RealOrder { get; private set; }

        public bool CanCloseOrder => RealOrder.Type == OrderType.Position || RealOrder.Type == OrderType.Market;

        public bool IsNull => Volume.E(0.0);

        public bool IsStopOrder => Type == OrderType.Stop || Type == OrderType.StopLimit;

        public bool IsImmediateFill => Type == OrderType.Market || (Type == OrderType.Limit && Options.HasFlag(OrderExecOptions.ImmediateOrCancel));


        public TaskCompletionSource<bool> Opened { get; private set; }

        public TaskCompletionSource<bool> OpenedGrossPosition { get; private set; }

        public TaskCompletionSource<bool> Filled { get; private set; }

        public TaskCompletionSource<bool> FinalExecution => IsGrossAcc ? OpenedGrossPosition : Filled;


        public double SlippagePrecision { get; }

        public OrderType InitType { get; protected set; }

        public string RelatedId { get; private set; }

        public string Id
        {
            get => _id;
            set
            {
                RelatedId = _id;
                _id = value;
            }
        }

        //public Behavior Mode { get; set; }


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

        public OrderTemplate(TestParamsSet test, double volume) : base(test.Type, test.Side, test.Async)
        {
            //Mode = mode;
            ReqVolume = volume;
            RemVolume = volume;
            Volume = volume;
            Opened = new TaskCompletionSource<bool>();
            Filled = new TaskCompletionSource<bool>();
            OpenedGrossPosition = new TaskCompletionSource<bool>();
            FilledParts = new List<OrderTemplate>();

            Options = test.Options;

            InitType = Type;
            SlippagePrecision = Math.Pow(10, Math.Max(Symbol.Digits, 4));
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

        //internal void ToOpen(Order order)
        //{
        //    Id = order.Id;
        //    RealOrder = Orders[Id];
        //}

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

        public void UpdateTemplate(Order order, bool activate = false, bool position = false)
        {
            Id = order.Id;
            RealOrder = Orders[Id];

            if (activate && Type == OrderType.StopLimit)
                Type = OrderType.Limit;
            else
                Type = position ? OrderType.Position : Type;

            Verification();
        }

        public void Verification(bool close = false)
        {
            if (close || IsImmediateFill || Type == OrderType.Position) //to do: create verification for Position
                return;

            if (RealOrder.IsNull != close)
                throw new VerificationException(Id, close);

            if (RealOrder.Type != Type)
                ThrowVerificationException(nameof(RealOrder.Type), Type, RealOrder.Type);

            if (RealOrder.Side != Side)
                ThrowVerificationException(nameof(RealOrder.Side), Side, RealOrder.Side);

            if (!RealOrder.RemainingVolume.EI(Volume))
                ThrowVerificationException(nameof(RealOrder.RemainingVolume), Volume, RealOrder.RemainingVolume);

            if (Type != OrderType.Stop && !CheckPrice(RealOrder.Price))
                ThrowVerificationException(nameof(RealOrder.Price), Price, RealOrder.Price);

            if (IsStopOrder && !RealOrder.StopPrice.EI(StopPrice))
                ThrowVerificationException(nameof(RealOrder.StopPrice), StopPrice, RealOrder.StopPrice);

            if (!RealOrder.MaxVisibleVolume.EI(MaxVisibleVolume) && !(-1.0).EI(MaxVisibleVolume) && !double.IsNaN(RealOrder.MaxVisibleVolume))
                ThrowVerificationException(nameof(RealOrder.MaxVisibleVolume), MaxVisibleVolume, RealOrder.MaxVisibleVolume);

            if (!RealOrder.TakeProfit.EI(TP) && !0.0.EI(TP) && !double.IsNaN(RealOrder.TakeProfit))
                ThrowVerificationException(nameof(RealOrder.TakeProfit), TP, RealOrder.TakeProfit);

            if (!RealOrder.StopLoss.EI(SL) && !0.0.EI(SL) && !double.IsNaN(RealOrder.StopLoss))
                ThrowVerificationException(nameof(RealOrder.StopLoss), SL, RealOrder.StopLoss);

            if (IsSupportedSlippage)
                CheckSlippage(RealOrder.Slippage, (realSlippage, expectedSlippage) =>
                {
                    if (!realSlippage.E(expectedSlippage))
                        ThrowVerificationException(nameof(RealOrder.Slippage), expectedSlippage, realSlippage);
                });

            if (IsSupportedOcO)
                if (OcoRelatedOrderId != RealOrder.OcoRelatedOrderId)
                    ThrowVerificationException(nameof(RealOrder.OcoRelatedOrderId), OcoRelatedOrderId, RealOrder.OcoRelatedOrderId);

            if (Comment != null && RealOrder.Comment != Comment)
                ThrowVerificationException(nameof(RealOrder.Comment), Comment, RealOrder.Comment);

            if (Expiration != null && RealOrder.Expiration != Expiration)
                ThrowVerificationException(nameof(RealOrder.Expiration), Expiration, RealOrder.Expiration);


            if (RealOrder.Tag != Tag)
                ThrowVerificationException(nameof(RealOrder.Tag), Tag, RealOrder.Tag);
        }

        public double? GetSlippageInPercent()
        {
            if (Slippage != null)
                Slippage = SlippageConverters.SlippagePipsToFractions(Slippage.Value, (IsStopOrder ? StopPrice : Price).Value, Symbol);

            return Slippage;
        }


        public string GetInfo(TestOrderAction action) => action != TestOrderAction.Open ? $"{action}{GetAllProperties()}" : base.GetInfo();

        private void ThrowVerificationException(string property, object expected, object current) => throw new VerificationException(RealOrder.Id, property, expected, current);

        private bool CheckPrice(double cur)
        {
            if (IsImmediateFill || Type == OrderType.Position)
                return Side == OrderSide.Buy ? cur.LteI(Price) : cur.GteI(Price);

            return cur.EI(Price);
        }

        protected double CeilSlippage(double slippage) => Math.Round(Math.Ceiling(slippage * SlippagePrecision) / SlippagePrecision, Symbol.Digits);

        protected double GetMaxSlippage() => SlippageConverters.SlippagePipsToFractions(Symbol.Slippage, (IsStopOrder ? StopPrice : Price).Value, Symbol);

        protected void CheckSlippage(double serverSlippage, Action<double, double> comparer)
        {
            var calcSlippage = GetMaxSlippage();
            var expectedSlippage = CeilSlippage(Slippage == null || Slippage.Value > calcSlippage ? calcSlippage : Slippage.Value);

            comparer(expectedSlippage, CeilSlippage(serverSlippage));
        }

        private string GetAllProperties()
        {
            var str = new StringBuilder(1 << 10);

            if (Id != null)
                str.Append($" order {Id}");

            SetProperty(str, Price, nameof(Price));
            SetProperty(str, Volume, nameof(Volume));
            SetProperty(str, Side, nameof(Side));
            SetProperty(str, InitType, nameof(Type)); //InitType as Type
            SetProperty(str, StopPrice, nameof(StopPrice));
            SetProperty(str, MaxVisibleVolume, nameof(MaxVisibleVolume));
            SetProperty(str, SL, nameof(SL));
            SetProperty(str, TP, nameof(TP));
            SetProperty(str, Slippage, nameof(Slippage));

            SetProperty(str, Expiration, nameof(Expiration));
            SetProperty(str, Comment, nameof(Comment));
            SetProperty(str, OcoRelatedOrderId, nameof(OcoRelatedOrderId));

            str.Append($", options={Options}");
            str.Append($", async={Async}.");

            return str.ToString();
        }

        private static void SetProperty(StringBuilder str, double? value, string name)
        {
            if (value != null && !double.IsNaN(value.Value) && !value.Value.LteI(0.0))
                str.Append($", {name}={value.Value}");
        }

        private static void SetProperty(StringBuilder str, object value, string name)
        {
            if (value != null && !string.IsNullOrEmpty(value as string))
                str.Append($", {name}={value}");
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

        public OrderTemplate InversedCopy(double? volume = null)
        {
            var copy = Copy();

            copy.Side = Side == OrderSide.Buy ? OrderSide.Sell : OrderSide.Buy;
            copy.Volume = volume ?? Volume;
            copy.RemVolume = copy.Volume;
            copy.ReqVolume = copy.Volume;

            return copy;
        }

        public static OrderTemplate operator +(OrderTemplate first, OrderTemplate second)
        {
            var resultCopy = first.Volume > second.Volume ? first.Copy() : second.Copy();

            resultCopy.Volume = first.Side == second.Side ?
                                first.Volume + second.Volume :
                                Math.Abs(first.Volume - second.Volume);

            resultCopy.RemVolume = resultCopy.Volume;
            resultCopy.ReqVolume = resultCopy.Volume;

            return resultCopy;
        }
    }
}
