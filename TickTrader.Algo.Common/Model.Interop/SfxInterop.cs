using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TickTrader.Algo.Common.Lib;
using TickTrader.Algo.Common.Model.Interop;
using TickTrader.Algo.Core;
using SFX = TickTrader.FDK;

namespace TickTrader.Algo.Common.Model
{
    class SfxInterop : IServerInterop
    {
        private const int ConnectTimeoutMs = 60 * 1000;
        private const int LogoutTimeoutMs = 60 * 1000;
        private const int DisconnectTimeoutMs = 60 * 1000;

        public IFeedServerApi FeedApi => throw new NotImplementedException();
        public ITradeServerApi TradeApi => throw new NotImplementedException();

        private SFX.QuoteFeed.Client _feedProxy;
        private SFX.OrderEntry.Client _tradeProxy;

        public event Action<IServerInterop, ConnectionErrorCodes> Disconnected;

        public SfxInterop()
        {
            _feedProxy = new SFX.QuoteFeed.Client("feed.proxy");
            _tradeProxy = new SFX.OrderEntry.Client("trade.proxy");
        }

        public async Task<ConnectionErrorCodes> Connect(string address, string login, string password, CancellationToken cancelToken)
        {
            await Task.WhenAll(
                ConnectFeed(address, login, password),
                ConnectTrade(address, login, password));

            return ConnectionErrorCodes.None;
        }

        private async Task ConnectFeed(string address, string login, string password)
        {
            await Task.Factory.StartNew(() => _feedProxy.Connect(address, ConnectTimeoutMs));
            await _feedProxy.LoginAsync(null, address, password, "", Guid.NewGuid().ToString());
        }

        private async Task ConnectTrade(string address, string login, string password)
        {
            await Task.Factory.StartNew(() => _feedProxy.Connect(address, ConnectTimeoutMs));
            await _tradeProxy.LoginAsync(null, address, password, "", Guid.NewGuid().ToString());
        }

        public Task Disconnect()
        {
            return Task.WhenAll(
                DisconnectFeed(),
                DisconnectTrade());
        }

        private async Task DisconnectFeed()
        {
            await _feedProxy.LogoutAsync(null, "")
                .AddTimeout(LogoutTimeoutMs);
            await Task.Factory.StartNew(() => _feedProxy.Disconnect(""));
        }

        private async Task DisconnectTrade()
        {
            await _feedProxy.LogoutAsync(null, "")
                .AddTimeout(LogoutTimeoutMs);
            await Task.Factory.StartNew(() => _feedProxy.Disconnect(""));
        }
    }
}
