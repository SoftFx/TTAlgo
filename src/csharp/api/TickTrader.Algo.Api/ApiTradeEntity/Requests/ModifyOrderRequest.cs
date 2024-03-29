﻿using System;

namespace TickTrader.Algo.Api
{
    public class ModifyOrderRequest
    {
        public string OrderId { get; private set; }

        public double? Volume { get; private set; }

        public double? Price { get; private set; }

        public double? StopPrice { get; private set; }

        public double? MaxVisibleVolume { get; private set; }

        public double? StopLoss { get; private set; }

        public double? TakeProfit { get; private set; }

        public double? Slippage { get; private set; }

        public DateTime? Expiration { get; private set; }

        public OrderExecOptions? Options { get; private set; }

        public string Comment { get; private set; }

        public string Tag { get; private set; }

        public bool? OcoEqualVolume { get; private set; }

        public string OcoRelatedOrderId { get; private set; }

        public ContingentOrderTrigger OtoTrigger { get; private set; }


        private ModifyOrderRequest() { }

        public class Template
        {
            private string _orderId;
            private double? _volume;
            private double? _price;
            private double? _stopPrice;
            private double? _maxVisibleVolume;
            private double? _stopLoss;
            private double? _takeProfit;
            private double? _slippage;
            private DateTime? _expiration;
            private OrderExecOptions? _options;
            private string _comment;
            private string _tag;

            private bool? _ocoEqualVolume;
            private string _ocoRelatedOrderId;

            private ContingentOrderTrigger _otoTrigger;


            private Template() { }


            public static Template Create() => new Template();

            public Template Copy() => (Template)MemberwiseClone();

            public ModifyOrderRequest MakeRequest()
            {
                return new ModifyOrderRequest
                {
                    OrderId = _orderId,
                    Volume = _volume,
                    Price = _price,
                    StopPrice = _stopPrice,
                    MaxVisibleVolume = _maxVisibleVolume,
                    StopLoss = _stopLoss,
                    TakeProfit = _takeProfit,
                    Slippage = _slippage,
                    Expiration = _expiration,
                    Options = _options,
                    Comment = _comment,
                    Tag = _tag,
                    OcoEqualVolume = _ocoEqualVolume,
                    OcoRelatedOrderId = _ocoRelatedOrderId,
                    OtoTrigger = _otoTrigger,
                };
            }

            public Template WithParams(string orderId, double? price)
            {
                return WithParams(orderId, price, null, null, null, null, null, null, null, null, null);
            }

            public Template WithParams(string orderId, double? price, double? volume)
            {
                return WithParams(orderId, price, null, volume, null, null, null, null, null, null, null);
            }

            public Template WithParams(string orderId, double? price, double? takeProfit, double? stopLoss)
            {
                return WithParams(orderId, price, null, null, null, takeProfit, stopLoss, null, null, null, null);
            }

            public Template WithParams(string orderId, double? price, double? stopPrice, double? volume, double? maxVisibleVolume, double? takeProfit, double? stopLoss, string comment, DateTime? expiration, OrderExecOptions? options, double? slippage)
            {
                _orderId = orderId;
                _volume = volume;
                _price = price;
                _stopPrice = stopPrice;
                _maxVisibleVolume = maxVisibleVolume;
                _stopLoss = stopLoss;
                _takeProfit = takeProfit;
                _slippage = slippage;
                _expiration = expiration;
                _options = options;
                _comment = comment;

                return this;
            }


            public Template WithOrderId(string orderId)
            {
                _orderId = orderId;
                return this;
            }

            public Template WithVolume(double? volume)
            {
                _volume = volume;
                return this;
            }

            public Template WithPrice(double? price)
            {
                _price = price;
                return this;
            }

            public Template WithStopPrice(double? stopPrice)
            {
                _stopPrice = stopPrice;
                return this;
            }

            public Template WithMaxVisibleVolume(double? maxVisibleVolume)
            {
                _maxVisibleVolume = maxVisibleVolume;
                return this;
            }

            public Template WithStopLoss(double? stopLoss)
            {
                _stopLoss = stopLoss;
                return this;
            }

            public Template WithTakeProfit(double? takeProfit)
            {
                _takeProfit = takeProfit;
                return this;
            }

            public Template WithSlippage(double? slippage)
            {
                _slippage = slippage;
                return this;
            }

            public Template WithExpiration(DateTime? expiration)
            {
                _expiration = expiration;
                return this;
            }

            public Template WithOptions(OrderExecOptions? options)
            {
                _options = options;
                return this;
            }

            public Template WithComment(string comment)
            {
                _comment = comment;
                return this;
            }

            public Template WithTag(string tag)
            {
                _tag = tag;
                return this;
            }

            public Template WithOCOParams(string relatedId, bool equalVolume = true)
            {
                return WithOptions(_options | OrderExecOptions.OneCancelsTheOther).WithOCOEqualVolume(equalVolume).WithOCORelatedOrderId(relatedId);
            }

            public Template WithOCOEqualVolume(bool? equalVolume)
            {
                _ocoEqualVolume = equalVolume;
                return this;
            }

            public Template WithOCORelatedOrderId(string relatedId)
            {
                if (!string.IsNullOrEmpty(relatedId))
                    _options = _options | OrderExecOptions.OneCancelsTheOther ?? OrderExecOptions.OneCancelsTheOther;

                _ocoRelatedOrderId = relatedId;
                return this;
            }

            public Template WithContingentOrderTrigger(ContingentOrderTrigger otoTrigger)
            {
                _otoTrigger = otoTrigger;
                return this;
            }
        }
    }
}
