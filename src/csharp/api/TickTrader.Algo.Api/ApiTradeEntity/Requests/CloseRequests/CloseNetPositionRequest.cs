namespace TickTrader.Algo.Api
{
    public class CloseNetPositionRequest : BaseCloseRequest
    {
        public string Symbol { get; private set; }


        private CloseNetPositionRequest() { }

        public class Template : BaseTemplate<Template>
        {
            private Template() { }

            public static Template Create() => new Template();


            public CloseNetPositionRequest MakeRequest()
            {
                return new CloseNetPositionRequest
                {
                    Symbol = _entityId,
                    Volume = _volume,
                    Slippage = _slippage,
                };
            }

            public Template WithParams(string symbol, double? volume, double? slippage)
            {
                _entityId = symbol;

                return WithParams(volume, slippage);
            }

            public Template WithSymbol(string symbol)
            {
                _entityId = symbol;

                return this;
            }

            public Template WithParams(string symbol)
            {
                return WithParams(symbol, null, null);
            }

            public Template WithParams(string symbol, double? volume)
            {
                return WithParams(symbol, volume, null);
            }
        }
    }
}
