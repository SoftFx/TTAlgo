namespace TickTrader.Algo.Api
{
    public class CloseOrderRequest : BaseCloseRequest
    {
        public string OrderId { get; private set; }


        private CloseOrderRequest() { }

        public class Template : BaseTemplate<Template>
        {
            private Template() { }

            public static Template Create() => new Template();


            public CloseOrderRequest MakeRequest()
            {
                return new CloseOrderRequest
                {
                    OrderId = _entityId,
                    Volume = _volume,
                    Slippage = _slippage,
                };
            }

            public Template WithParams(string orderId, double? volume, double? slippage)
            {
                _entityId = orderId;

                return WithParams(volume, slippage);
            }

            public Template WithOrderId(string orderId)
            {
                _entityId = orderId;

                return this;
            }

            public Template WithParams(string orderId)
            {
                return WithParams(orderId, null, null);
            }

            public Template WithParams(string orderId, double? volume)
            {
                return WithParams(orderId, volume, null);
            }
        }
    }
}
