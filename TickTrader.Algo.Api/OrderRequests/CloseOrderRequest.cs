using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Api
{
    public class CloseOrderRequest
    {
        public string OrderId { get; private set; }

        public double? Volume { get; private set; }

        public double? Slippage { get; private set; }


        private CloseOrderRequest() { }

        public class Template
        {
            private string _orderId;
            private double? _volume;
            private double? _slippage;


            private Template() { }


            public static Template Create() => new Template();

            public Template Copy() => (Template)MemberwiseClone();

            public CloseOrderRequest MakeRequest()
            {
                return new CloseOrderRequest
                {
                    OrderId = _orderId,
                    Volume = _volume,
                    Slippage = _slippage,
                };
            }

            public Template WithParams(string orderId)
            {
                return WithParams(orderId, null, null);
            }

            public Template WithParams(string orderId, double? volume)
            {
                return WithParams(orderId, volume, null);
            }

            public Template WithParams(string orderId, double? volume, double? slippage)
            {
                _orderId = orderId;
                _volume = volume;
                _slippage = slippage;

                return this;
            }


            public Template WithOrderId(string orderId)
            {
                _orderId = orderId;
                return this;
            }

            public Template WithVolume(double volume)
            {
                _volume = volume;
                return this;
            }

            public Template WithSlippage(double? slippage)
            {
                _slippage = slippage;
                return this;
            }
        }
    }
}
