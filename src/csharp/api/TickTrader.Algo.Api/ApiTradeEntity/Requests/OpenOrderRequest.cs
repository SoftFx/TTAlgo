using System;

namespace TickTrader.Algo.Api
{
    public class OpenOrderRequest
    {
        public string Symbol { get; private set; }

        public OrderType Type { get; private set; }

        public OrderSide Side { get; private set; }

        public double Volume { get; private set; }

        public double? Price { get; private set; }

        public double? StopPrice { get; private set; }

        public double? MaxVisibleVolume { get; private set; }

        public double? StopLoss { get; private set; }

        public double? TakeProfit { get; private set; }

        public double? Slippage { get; private set; }

        public DateTime? Expiration { get; private set; }

        public OrderExecOptions Options { get; private set; }

        public string Comment { get; private set; }

        public string Tag { get; private set; }

        public bool OcoEqualVolume { get; private set; }

        public string OcoRelatedOrderId { get; private set; }

        public ContingentOrderTrigger OtoTrigger { get; private set; }


        private OpenOrderRequest() { }


        public class Template
        {
            private string _symbol;
            private OrderType _type;
            private OrderSide _side;
            private double _volume;
            private double? _price;
            private double? _stopPrice;
            private double? _maxVisibleVolume;
            private double? _stopLoss;
            private double? _takeProfit;
            private double? _slippage;
            private DateTime? _expiration;
            private OrderExecOptions _options;
            private string _comment;
            private string _tag;

            private bool _ocoEqualVolume;
            private string _ocoRelatedOrderId;

            private ContingentOrderTrigger _otoTrigger;


            private Template() { }


            public static Template Create() => new Template();

            public Template Copy() => (Template)MemberwiseClone();

            public OpenOrderRequest MakeRequest()
            {
                return new OpenOrderRequest
                {
                    Symbol = _symbol,
                    Type = _type,
                    Side = _side,
                    Volume = _volume,
                    Price = _type != OrderType.Stop ? _price : null,
                    StopPrice = (_type == OrderType.Stop || _type == OrderType.StopLimit) ? _stopPrice : null,
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

            public Template WithParams(string symbol, OrderSide side, OrderType type, double volume, double? price, double? stopPrice)
            {
                return WithParams(symbol, side, type, volume, price, stopPrice, null, null, null, null, OrderExecOptions.None, null, null, null);
            }

            public Template WithParams(string symbol, OrderSide side, OrderType type, double volume, double? price, double? stopPrice, string comment)
            {
                return WithParams(symbol, side, type, volume, price, stopPrice, null, null, null, comment, OrderExecOptions.None, null, null, null);
            }

            public Template WithParams(string symbol, OrderSide side, OrderType type, double volume, double? price, double? stopPrice, double? maxVisibleVolume)
            {
                return WithParams(symbol, side, type, volume, price, stopPrice, maxVisibleVolume, null, null, null, OrderExecOptions.None, null, null, null);
            }

            public Template WithParams(string symbol, OrderSide side, OrderType type, double volume, double? price, double? stopPrice, double? maxVisibleVolume, string comment)
            {
                return WithParams(symbol, side, type, volume, price, stopPrice, maxVisibleVolume, null, null, comment, OrderExecOptions.None, null, null, null);
            }

            public Template WithParams(string symbol, OrderSide side, OrderType type, double volume, double? price, double? stopPrice, double? takeProfit, double? stopLoss)
            {
                return WithParams(symbol, side, type, volume, price, stopPrice, null, takeProfit, stopLoss, null, OrderExecOptions.None, null, null, null);
            }

            public Template WithParams(string symbol, OrderSide side, OrderType type, double volume, double? price, double? stopPrice, double? takeProfit, double? stopLoss, string comment)
            {
                return WithParams(symbol, side, type, volume, price, stopPrice, null, takeProfit, stopLoss, comment, OrderExecOptions.None, null, null, null);
            }

            public Template WithParams(string symbol, OrderSide side, OrderType type, double volume, double? price, double? stopPrice, double? maxVisibleVolume, double? takeProfit, double? stopLoss, string comment, OrderExecOptions options, string tag, DateTime? expiration, double? slippage)
            {
                _symbol = symbol;
                _type = type;
                _side = side;
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
                _tag = tag;

                return this;
            }


            public Template WithSymbol(string symbol)
            {
                _symbol = symbol;
                return this;
            }

            public Template WithType(OrderType type)
            {
                _type = type;
                return this;
            }

            public Template WithSide(OrderSide side)
            {
                _side = side;
                return this;
            }

            public Template WithVolume(double volume)
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

            public Template WithOptions(OrderExecOptions options)
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

            public Template WithOCOParams(bool equalVolume, string relatedId)
            {
                return WithOptions(_options | OrderExecOptions.OneCancelsTheOther).WithOCOEqualVolume(equalVolume).WithOCORelatedOrderId(relatedId);
            }

            public Template WithOCOEqualVolume(bool equalVolume)
            {
                _ocoEqualVolume = equalVolume;
                return this;
            }

            public Template WithOCORelatedOrderId(string relatedId)
            {
                _ocoRelatedOrderId = relatedId;
                return this;
            }

            public Template WithContigentOrderTrigger(ContingentOrderTrigger otoTrigger)
            {
                _otoTrigger = otoTrigger;
                return this;
            }
        }
    }
}
