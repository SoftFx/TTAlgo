using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TickTrader.Algo.Api;
using TickTrader.Algo.Api.Math;

namespace TickTrader.Algo.TestCollection.CompositeApiTest
{
    internal class OrderStateTemplate : OrderTemplate
    {
        public TaskCompletionSource<bool> Opened { get; private set; }

        public TaskCompletionSource<bool> OpenedGrossPosition { get; private set; }

        public TaskCompletionSource<bool> RejectOpened { get; private set; }

        public TaskCompletionSource<bool> OnTimeTriggerReceived { get; private set; }

        public TaskCompletionSource<bool> Filled { get; private set; }

        public TaskCompletionSource<bool> Canceled { get; private set; }

        public TaskCompletionSource<bool> Modified { get; private set; }

        public TaskCompletionSource<bool> Expired { get; private set; }

        public TaskCompletionSource<bool> Closed { get; private set; }


        public TaskCompletionSource<bool> IsExecuted => IsGrossAcc ? OpenedGrossPosition : Filled;

        public bool IsRemoved
        {
            get
            {
                if (!Opened.Task.IsCompleted || RejectOpened.Task.IsCompleted)
                    return true;

                if (IsGrossAcc)
                    return OpenedGrossPosition.Task.IsCompleted && Closed.Task.IsCompleted;
                else
                    return Filled.Task.IsCompleted || Canceled.Task.IsCompleted || Expired.Task.IsCompleted;
            }
        }

        public List<OrderStateTemplate> FilledParts { get; private set; }


        public OrderStateTemplate() { }

        public OrderStateTemplate(OrderBaseSet test, double volume) : base(test, volume)
        {
            ResetTemplateStates();
        }


        private void ResetTemplateStates()
        {
            Opened = new TaskCompletionSource<bool>();
            OpenedGrossPosition = new TaskCompletionSource<bool>();
            RejectOpened = new TaskCompletionSource<bool>();

            Filled = new TaskCompletionSource<bool>();
            FilledParts = new List<OrderStateTemplate>();
            Canceled = new TaskCompletionSource<bool>();
            Modified = new TaskCompletionSource<bool>();
            Expired = new TaskCompletionSource<bool>();
            Closed = new TaskCompletionSource<bool>();

            OnTimeTriggerReceived = new TaskCompletionSource<bool>();
        }

        internal OrderStateTemplate ToOpen(string orderId)
        {
            if (Canceled.Task.IsCompleted)
                Canceled = new TaskCompletionSource<bool>(); //for triggers

            Id = orderId;
            Opened.SetResult(true);

            return this;
        }

        internal OrderStateTemplate ToRejectOpen()
        {
            RejectOpened.SetResult(true);

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

        internal OrderStateTemplate ToModified()
        {
            Modified.TrySetResult(true);

            return this;
        }

        internal OrderStateTemplate ToGrossPosition()
        {
            Type = OrderType.Position;

            OpenedGrossPosition.SetResult(true);

            return this;
        }

        internal OrderStateTemplate ToCancel()
        {
            Canceled.SetResult(true);

            return this;
        }

        internal OrderStateTemplate ToClose()
        {
            Closed.SetResult(true);

            return this;
        }

        internal OrderStateTemplate ToExpire()
        {
            Expired.SetResult(true);

            return this;
        }

        internal OrderStateTemplate ToOnTimeTriggerReceived()
        {
            Opened = new TaskCompletionSource<bool>();
            TriggerType = null;

            OnTimeTriggerReceived.SetResult(true);

            return this;
        }

        internal void OnFinalEvent(string orderId, Type eventType)
        {
            var ex = new Exception($"Order #{orderId} recieved final event {eventType.Name}");

            Opened.TrySetException(ex);
            OpenedGrossPosition.TrySetException(ex);
            RejectOpened.TrySetException(ex);
            OnTimeTriggerReceived.TrySetException(ex);
            Filled.TrySetException(ex);
            Canceled.TrySetException(ex);
            Modified.TrySetException(ex);
            Expired.TrySetException(ex);
            Closed.TrySetException(ex);
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

            copy.ResetTemplateStates();
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
