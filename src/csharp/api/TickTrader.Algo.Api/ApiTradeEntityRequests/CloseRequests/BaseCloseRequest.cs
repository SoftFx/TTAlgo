namespace TickTrader.Algo.Api
{
    public abstract class BaseCloseRequest
    {
        public double? Volume { get; protected set; }

        public double? Slippage { get; protected set; }


        public abstract class BaseTemplate<T> where T : BaseTemplate<T>
        {
            protected string _entityId;

            protected double? _volume;
            protected double? _slippage;

            public T WithParams(double? volume, double? slippage)
            {
                _volume = volume;
                _slippage = slippage;

                return (T)this;
            }

            public T WithVolume(double? volume)
            {
                _volume = volume;

                return (T)this;
            }

            public T WithSlippage(double? slippage)
            {
                _slippage = slippage;

                return (T)this;
            }

            public T Copy() => (T)MemberwiseClone();
        }
    }
}
