using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TickTrader.Algo.Api;
using TickTrader.Algo.Api.Math;

namespace TickTrader.Algo.TestCollection.CompositeApiTest
{
    internal class OrderStateTemplate : OrderTemplate
    {
        public Order RealOrder { get; private set; }

        public bool CanCloseOrder => RealOrder.Type == OrderType.Position || RealOrder.Type == OrderType.Market;


        public TaskCompletionSource<bool> Opened { get; private set; }

        public TaskCompletionSource<bool> OpenedGrossPosition { get; private set; }

        public TaskCompletionSource<bool> Filled { get; private set; }

        public TaskCompletionSource<bool> FinalExecution => IsGrossAcc ? OpenedGrossPosition : Filled;


        public List<OrderStateTemplate> FilledParts { get; private set; }


        public OrderStateTemplate() { }

        public OrderStateTemplate(TestParamsSet test, double volume) : base(test, volume)
        {
            Opened = new TaskCompletionSource<bool>();
            Filled = new TaskCompletionSource<bool>();
            OpenedGrossPosition = new TaskCompletionSource<bool>();
            FilledParts = new List<OrderStateTemplate>();
        }


        internal OrderStateTemplate ToOpen(string orderId)
        {
            Id = orderId;
            RealOrder = Orders[Id];
            Opened.SetResult(true);

            return this;
        }

        internal OrderStateTemplate ToActivate()
        {
            Opened = new TaskCompletionSource<bool>();
            //InitType = OrderType.StopLimit;
            Type = OrderType.Limit;

            return this;
        }

        internal OrderStateTemplate ToFilled(double filledVolume)
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

        internal OrderStateTemplate ToGrossPosition()
        {
            Opened = new TaskCompletionSource<bool>();
            //InitType = Type;
            Type = OrderType.Position;
            OpenedGrossPosition.SetResult(true);

            return this;
        }


        internal OrderStateTemplate ForPending(int coef = 3)
        {
            Price = CalculatePrice(coef);
            StopPrice = CalculatePrice(-coef);

            return this;
        }

        internal OrderStateTemplate ForExecuting(int coef = 3)
        {
            Price = CalculatePrice(-coef);
            StopPrice = CalculatePrice(coef);

            return this;
        }

        internal OrderStateTemplate ForGrossPositionPending(int coef = 3, string comment = "")
        {
            TP = CalculatePrice(-coef);
            SL = CalculatePrice(coef);
            Comment = comment;

            return ForExecuting(coef);
        }


        //add deep copy trigger property later
        public OrderStateTemplate Copy(double? newVolume = null)
        {
            var copy = (OrderStateTemplate)MemberwiseClone();

            copy.Opened = new TaskCompletionSource<bool>();
            copy.Filled = new TaskCompletionSource<bool>();
            copy.FilledParts = new List<OrderStateTemplate>();
            copy.OpenedGrossPosition = new TaskCompletionSource<bool>();
            copy.LinkedOrders = new List<OrderTemplate>();
            copy.SetVolume(newVolume ?? Volume);

            return copy;
        }

        public static OrderStateTemplate operator !(OrderStateTemplate order)
        {
            order.Side = order.Side.Inversed();

            return order;
        }

        public static OrderStateTemplate operator +(OrderStateTemplate first, OrderStateTemplate second)
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
