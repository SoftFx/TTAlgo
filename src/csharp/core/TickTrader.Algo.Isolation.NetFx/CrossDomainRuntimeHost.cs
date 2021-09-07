using System;
using System.Threading.Tasks;

namespace TickTrader.Algo.Isolation.NetFx
{
    //public class CrossDomainRuntimeHost : IRuntimeHostProxy, IDisposable
    //{
    //    private Isolated<RuntimeV1Loader> _subDomain;


    //    public async Task Start(string address, int port, string proxyId)
    //    {
    //        _subDomain = new Isolated<RuntimeV1Loader>();

    //        _subDomain.Value.InitDebugLogger();
    //        await Task.Factory.StartNew(() => _subDomain.Value.Init(address, port, proxyId));
    //    }

    //    public async Task Stop()
    //    {
    //        await Task.Delay(2000); // ugly hack to give code in another domain some time for correct stop without thread abort

    //        await Task.Factory.StartNew(() => _subDomain.Value.Deinit());

    //        _subDomain?.Dispose();
    //        _subDomain = null;
    //    }


    //    public void Dispose()
    //    {
    //        _subDomain?.Dispose();
    //    }
    //}
}
