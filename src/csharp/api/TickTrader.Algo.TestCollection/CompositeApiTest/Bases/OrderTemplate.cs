﻿using System;
using System.Collections.Generic;
using System.Linq;
using TickTrader.Algo.Api;
using TickTrader.Algo.Api.Math;

using TriggerTypes = TickTrader.Algo.Api.ContingentOrderTrigger.TriggerType;

namespace TickTrader.Algo.TestCollection.CompositeApiTest
{
    internal abstract class OrderTemplate : OrderBaseSet
    {
        private readonly double _slippagePrecision;
        private DateTime? _expiration;


        public bool IsNull => Volume.E(0.0);

        public bool IsOnTimeTrigger => TriggerType == TriggerTypes.OnTime;


        public OrderType InitType { get; }


        public string Id { get; protected set; } = string.Empty;

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

        public bool OcoEqualVolume { get; set; }

        internal OrderStateTemplate RelatedOcoTemplate { get; private set; }

        internal List<OrderStateTemplate> LinkedOrders { get; set; }


        public TriggerTypes? TriggerType { get; set; }

        public DateTime? TriggerTime { get; set; }

        public string OrderIdTriggeredBy { get; set; }


        internal OrderTemplate() { }

        internal OrderTemplate(OrderBaseSet set, double volume) : base(set.Type, set.Side)
        {
            _slippagePrecision = Math.Pow(10, Math.Max(Symbol.Digits, 4));

            LinkedOrders = new List<OrderStateTemplate>();

            InitType = Type;
            Options = set.Options;

            SetVolume(volume);
        }


        internal double? CalculatePrice(int coef)
        {
            var delta = coef * PriceDelta * Symbol.Point;

            return Side.IsBuy() ? Symbol.Ask - delta : Symbol.Bid + delta;
        }

        protected void SetVolume(double volume)
        {
            ReqVolume = volume;
            RemVolume = volume;
            Volume = volume;
        }

        internal OrderStateTemplate WithOCO(OrderTemplate ocoOrder)
       {
            void SetOCO(OrderTemplate main, OrderTemplate oco)
            {
                if (!main.Options.HasFlag(OrderExecOptions.OneCancelsTheOther))
                {
                    main.Options |= OrderExecOptions.OneCancelsTheOther;
                    main.OcoRelatedOrderId = oco.Id;
                    main.RelatedOcoTemplate = (OrderStateTemplate)oco;
                }
            }

            SetOCO(ocoOrder, this);
            SetOCO(this, ocoOrder);

            return (OrderStateTemplate)this;
        }

        internal OrderStateTemplate WithExpiration(int seconds)
        {
            Expiration = UtcNow + TimeSpan.FromSeconds(seconds);

            return (OrderStateTemplate)this;
        }

        internal OrderStateTemplate WithLinkedOCO(OrderStateTemplate linkedOrder)
        {
            Options |= OrderExecOptions.OneCancelsTheOther;
            LinkedOrders.Add(linkedOrder.WithOCO((OrderStateTemplate)this).WithFullLinkedOCOProperies());
            RelatedOcoTemplate = linkedOrder;

            return WithFullLinkedOCOProperies();
        }

        internal OrderStateTemplate BreakOCO()
        {
            if (RelatedOcoTemplate == this)
                RelatedOcoTemplate.RemoveOCO();

            return RemoveOCO();
        }

        internal OrderStateTemplate RemoveOCO()
        {
            Options &= ~OrderExecOptions.OneCancelsTheOther;
            OcoRelatedOrderId = null;
            RelatedOcoTemplate = null;

            return (OrderStateTemplate)this;
        }

        internal OrderStateTemplate FillAdditionalProperties(double coef = 0.9)
        {
            Comment = $"{Options}";

            if (TriggerType != null)
                Comment += $" TriggerType = {TriggerType}";

            if (IsSupportedSlippage)
                Slippage = Symbol.Slippage * coef;

            if (IsSupportedMaxVisibleVolume)
                MaxVisibleVolume = Volume * coef;

            return (OrderStateTemplate)this;
        }

        private OrderStateTemplate WithFullLinkedOCOProperies() => FillAdditionalProperties().WithExpiration(60);

        internal OrderStateTemplate WithOnTimeTrigger(int seconds)
        {
            var utcNow = UtcNow;

            return WithOnTimeTrigger((utcNow + TimeSpan.FromSeconds(seconds)).AddMilliseconds(-utcNow.Millisecond));
        }

        internal OrderStateTemplate WithOnTimeTrigger(DateTime? triggerTime)
        {
            TriggerType = TriggerTypes.OnTime;
            TriggerTime = triggerTime;

            return (OrderStateTemplate)this;
        }

        internal OrderStateTemplate RemoveOnTimeTrigger()
        {
            TriggerType &= ~TriggerTypes.OnTime;
            TriggerTime = null;

            return (OrderStateTemplate)this;
        }

        internal OrderStateTemplate WithOnExpiredTrigger(OrderStateTemplate order) => WithOnExpiredTrigger(order.Id);

        internal OrderStateTemplate WithOnExpiredTrigger(string orderTriggeredById)
        {
            TriggerType = TriggerTypes.OnPendingOrderExpired;
            OrderIdTriggeredBy = orderTriggeredById;

            return (OrderStateTemplate)this;
        }

        internal OrderStateTemplate WithOnPartialFilledTrigger(OrderStateTemplate order) => WithOnPartialFilledTrigger(order.Id);

        internal OrderStateTemplate WithOnPartialFilledTrigger(string orderTriggeredById)
        {
            TriggerType = TriggerTypes.OnPendingOrderPartiallyFilled;
            OrderIdTriggeredBy = orderTriggeredById;

            return (OrderStateTemplate)this;
        }



        internal OpenOrderRequest GetOpenRequest()
        {
            return OpenOrderRequest.Template.Create()
                   .WithParams(Symbol.Name, Side, Type, Volume, Price, StopPrice)
                   .WithSubOpenRequests(LinkedOrders.Select(u => u.GetOpenRequest())?.ToArray())
                   .WithContingentOrderTrigger(BuildTrigger())
                   .WithOCORelatedOrderId(OcoRelatedOrderId)
                   .WithMaxVisibleVolume(MaxVisibleVolume)
                   .WithSlippage(GetSlippageInPercent())
                   .WithOCOEqualVolume(OcoEqualVolume)
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
                   .WithContingentOrderTrigger(BuildTrigger())
                   .WithOCORelatedOrderId(OcoRelatedOrderId)
                   .WithMaxVisibleVolume(MaxVisibleVolume)
                   .WithSlippage(GetSlippageInPercent())
                   .WithOCOEqualVolume(OcoEqualVolume)
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

        private ContingentOrderTrigger BuildTrigger()
        {
            return TriggerType != null ? ContingentOrderTrigger.Create(TriggerType.Value, TriggerTime, OrderIdTriggeredBy) : null;
        }

        private double? GetSlippageInPercent()
        {
            if (Slippage != null)
                Slippage = SlippageConverters.SlippagePipsToFractions(Slippage.Value, (IsSupportedStopPrice ? StopPrice : Price).Value, Symbol);

            return Slippage;
        }
    }
}