using System;
using System.Threading.Tasks;
using TickTrader.Algo.Core.Lib;

namespace TickTrader.Algo.Core
{
    public interface IRuntimeHostProxy
    {
        Task Start(string address, int port, string proxyId);

        Task Stop();
    }


    public static class RuntimeHost
    {
        public static IRuntimeHostProxy Create(bool isolated)
        {
            if (isolated)
                return new CrossDomainRuntimeHost();
            else
                return new TransparentRuntimeHost();
        }
    }


    public class TransparentRuntimeHost : IRuntimeHostProxy
    {
        public Task Start(string address, int port, string proxyId)
        {
            return Task.CompletedTask;
        }

        public Task Stop()
        {
            return Task.CompletedTask;
        }
    }


    public class CrossDomainRuntimeHost : IRuntimeHostProxy, IDisposable
    {
        private Isolated<RuntimeV1Loader> _subDomain;


        public async Task Start(string address, int port, string proxyId)
        {
            _subDomain = new Isolated<RuntimeV1Loader>();

            await Task.Factory.StartNew(() => _subDomain.Value.Init(address, port, proxyId));
        }

        public async Task Stop()
        {
            await Task.Factory.StartNew(() => _subDomain.Value.Deinit());

            _subDomain?.Dispose();
            _subDomain = null;
        }


        public void Dispose()
        {
            _subDomain?.Dispose();
        }
    }
}
