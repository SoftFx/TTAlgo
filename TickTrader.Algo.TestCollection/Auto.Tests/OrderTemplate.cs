using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.TestCollection.Auto.Tests
{
    public class OrderTemplate : TestParamsSet, ICloneable
    {
        private string _id;


        public Order RealOrder => Orders[Id];

        public bool IsCloseOrder => RealOrder.Type == OrderType.Position || RealOrder.Type == OrderType.Market;

        public bool IsStopOrder => Type == OrderType.Stop || Type == OrderType.StopLimit;

        public bool IsImmediateFill => Type == OrderType.Market ||
                                       (Type == OrderType.Limit && Options.HasFlag(OrderExecOptions.ImmediateOrCancel)) ||
                                       (Type == OrderType.Position && AccountType == AccountTypes.Gross);

        public bool IsIoc { get; }

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

        public OrderExecutionMode Mode { get; set; }


        public double? Price { get; set; }

        public double? StopPrice { get; set; }

        public double? MaxVisibleVolume { get; set; }

        public double? SL { get; set; }

        public double? TP { get; set; }

        public double? Volume { get; set; }

        public double? Slippage { get; set; }

        public DateTime? Expiration { get; set; }

        public string Comment { get; set; }


        public OrderTemplate() { }

        public OrderTemplate(TestParamsSet test, OrderExecutionMode mode) : base(test.Type, test.Side, test.Async)
        {
            Mode = mode;
            Options = test.Options;
            IsIoc = Type == OrderType.Limit && Options.HasFlag(OrderExecOptions.ImmediateOrCancel);
        }

        public void SetType(OrderType type)
        {
            Type = type;
        }

        public void UpdateTemplate(Order order, bool activate = false, bool position = false)
        {
            Id = order.Id;

            if (activate && Type == OrderType.StopLimit)
                Type = OrderType.Limit;
            else
                Type = position ? OrderType.Position : Type;

            Verification();
        }

        public void Verification(bool close = false)
        {
            if (close || IsImmediateFill)
                return;

            if (RealOrder.IsNull != close)
                throw new VerificationException(Id, close);

            if (RealOrder.Type != Type)
                throw new VerificationException(RealOrder.Id, nameof(RealOrder.Type), Type, RealOrder.Type);

            if (RealOrder.Side != Side)
                throw new VerificationException(RealOrder.Id, nameof(RealOrder.Side), Side, RealOrder.Side);

            if (!RealOrder.RemainingVolume.EI(Volume))
                throw new VerificationException(RealOrder.Id, nameof(RealOrder.RemainingVolume), Volume, RealOrder.RemainingVolume);

            if (Type != OrderType.Stop && !CheckPrice(RealOrder.Price))
                throw new VerificationException(RealOrder.Id, nameof(RealOrder.Price), Price, RealOrder.Price);

            if (IsStopOrder && !RealOrder.StopPrice.EI(StopPrice))
                throw new VerificationException(RealOrder.Id, nameof(RealOrder.StopPrice), StopPrice, RealOrder.StopPrice);

            if (!RealOrder.MaxVisibleVolume.EI(MaxVisibleVolume) && !(-1.0).EI(MaxVisibleVolume) && !double.IsNaN(RealOrder.MaxVisibleVolume))
                throw new VerificationException(RealOrder.Id, nameof(RealOrder.MaxVisibleVolume), MaxVisibleVolume, RealOrder.MaxVisibleVolume);

            if (!RealOrder.TakeProfit.EI(TP) && !0.0.EI(TP) && !double.IsNaN(RealOrder.TakeProfit))
                throw new VerificationException(RealOrder.Id, nameof(RealOrder.TakeProfit), TP, RealOrder.TakeProfit);

            if (!RealOrder.StopLoss.EI(SL) && !0.0.EI(SL) && !double.IsNaN(RealOrder.StopLoss))
                throw new VerificationException(RealOrder.Id, nameof(RealOrder.StopLoss), SL, RealOrder.StopLoss);


            if (Comment != null && RealOrder.Comment != Comment)
                throw new VerificationException(RealOrder.Id, nameof(RealOrder.Comment), Comment, RealOrder.Comment);

            if (Expiration != null && RealOrder.Expiration != Expiration)
                throw new VerificationException(RealOrder.Id, nameof(RealOrder.Expiration), Expiration, RealOrder.Expiration);


            if (RealOrder.Tag != Tag)
                throw new VerificationException(RealOrder.Id, nameof(RealOrder.Tag), Tag, RealOrder.Tag);
        }

        public string GetInfo(TestPropertyAction action, string property) => $"{action} {property}{GetAllProperties()}";

        public string GetInfo(TestOrderAction action) => action != TestOrderAction.Open ? $"{action}{GetAllProperties()}" : base.GetInfo();

        public string GetAction(TestPropertyAction action, string property) => $"{(Async ? "Async " : "")}{action} {property} {Type} {(Options == OrderExecOptions.ImmediateOrCancel ? "IoC" : "")} {Side} to order {Id}";

        private bool CheckPrice(double cur)
        {
            if (IsImmediateFill)
                return Side == OrderSide.Buy ? cur.LteI(Price) : cur.GteI(Price);

            return cur.EI(Price);
        }

        private string GetAllProperties()
        {
            var str = new StringBuilder();

            if (Id != null)
                str.Append($" order {Id}");

            SetProperty(str, Price, nameof(Price));
            SetProperty(str, Volume, nameof(Volume));
            SetProperty(str, Side, nameof(Side));
            SetProperty(str, Type, nameof(Type));
            SetProperty(str, StopPrice, nameof(StopPrice));
            SetProperty(str, MaxVisibleVolume, nameof(MaxVisibleVolume));
            SetProperty(str, SL, nameof(SL));
            SetProperty(str, TP, nameof(TP));
            SetProperty(str, Slippage, nameof(Slippage));

            SetProperty(str, Expiration, nameof(Expiration));
            SetProperty(str, Comment, nameof(Comment));

            str.Append($", options={Options}");
            str.Append($", async={Async}.");

            return str.ToString();
        }

        private void SetProperty(StringBuilder str, double? value, string name)
        {
            if (value != null && !double.IsNaN(value.Value) && !value.Value.LteI(0.0))
                str.Append($", {name}={value.Value}");
        }

        private void SetProperty(StringBuilder str, object value, string name)
        {
            if (value != null && !string.IsNullOrEmpty(value as string))
                str.Append($", {name}={value}");
        }

        public object Clone() => MemberwiseClone();

        public OrderTemplate InversedCopy(double? volume = null)
        {
            var copy = (OrderTemplate)Clone();

            copy.Side = Side == OrderSide.Buy ? OrderSide.Sell : OrderSide.Buy;
            copy.Volume = volume ?? Volume;

            return copy;
        }
    }
}
