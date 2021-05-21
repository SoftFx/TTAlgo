using System;
using System.Threading.Tasks;

namespace TickTrader.Algo.Core
{
    public interface IRuntimeHostProxy
    {
        Task Start(string address, int port, string proxyId);

        Task Stop();
    }


    public static class RuntimeHost
    {
        private static Func<bool, IRuntimeHostProxy> _factory;


        public static void Init(Func<bool, IRuntimeHostProxy> factory)
        {
            _factory = factory;
        }

        public static IRuntimeHostProxy Create(bool isolated)
        {
            return _factory(isolated);
        }
    }
}
