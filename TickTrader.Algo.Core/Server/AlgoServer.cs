using Google.Protobuf.WellKnownTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TickTrader.Algo.Core.Repository;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Rpc;
using TickTrader.Algo.Rpc.OverGrpc;

namespace TickTrader.Algo.Core
{
    public class AlgoServer : IRpcHost
    {
        private readonly Dictionary<string, PluginExecutor> _executorsMap;
        private readonly RpcServer _rpcServer;


        public string Address { get; } = "localhost";

        public int BoundPort => _rpcServer.BoundPort;


        public AlgoServer()
        {
            _executorsMap = new Dictionary<string, PluginExecutor>();
            _rpcServer = new RpcServer(new GrpcFactory(), this);
        }


        public async Task Start()
        {
            await _rpcServer.Start(Address, 0);
        }

        public async Task Stop()
        {
            await _rpcServer.Stop();
        }

        public PluginExecutor CreateExecutor(AlgoPluginRef pluginRef, ISynchronizationContext updatesSync)
        {
            var id = Guid.NewGuid().ToString("N");
            var executor = new PluginExecutor(id, pluginRef, updatesSync);
            _executorsMap.Add(id, executor);
            return executor;
        }


        #region IRpcHost implementation

        ProtocolSpec IRpcHost.Resolve(ProtocolSpec protocol, out string error)
        {
            error = string.Empty;
            return protocol;
        }

        IRpcHandler IRpcHost.GetRpcHandler(ProtocolSpec protocol)
        {
            switch (protocol.Type)
            {
                case ProtocolType.RemotePlugin:
                    return new ExecutorHandler(this);
            }
            return null;
        }

        #endregion IRpcHost implementation

        private class ExecutorHandler : IRpcHandler
        {
            private readonly AlgoServer _server;
            private PluginExecutor _executor;
            private RpcSession _session;


            public ExecutorHandler(AlgoServer server)
            {
                _server = server;
            }


            public void SetSession(RpcSession session)
            {
                _session = session;
            }

            public void HandleNotification(string callId, Any payload)
            {
                throw new NotImplementedException();
            }

            public Any HandleRequest(string callId, Any payload)
            {
                if (payload.Is(AttachPluginRequest.Descriptor))
                {
                    var request = payload.Unpack<AttachPluginRequest>();
                    if (_executor != null)
                    {
                        return Any.Pack(new RpcError { Message = "Executor already attached!" });
                    }
                    if (_server._executorsMap.TryGetValue(request.Id, out var executor))
                    {
                        _executor = executor;
                        ThreadPool.QueueUserWorkItem(_ =>
                        {
                            try
                            {
                                _executor.Start();
                            }
                            catch (Exception) { }
                        });
                        return Any.Pack(new AttachPluginResponse { Success = true });
                    }
                    else
                    {
                        return Any.Pack(new AttachPluginResponse { Success = false });
                    }
                }
                else if (payload.Is(CurrencyListRequest.Descriptor))
                    return CurrencyListRequestHandler();
                else if (payload.Is(SymbolListRequest.Descriptor))
                    return SymbolListRequestHandler();
                return null;
            }


            private Any CurrencyListRequestHandler()
            {
                var response = new CurrencyListResponse();
                response.Currencies.Add(
                    _executor.Metadata.GetCurrencyMetadata()
                    .Select(c => new Domain.CurrencyEntity
                    {
                        Name = c.Name,
                        Digits = c.Digits,
                    }));
                return Any.Pack(response);
            }

            private Any SymbolListRequestHandler()
            {
                var response = new SymbolListResponse();
                response.Symbols.Add(
                    _executor.Metadata.GetSymbolMetadata()
                    .Select(s => new Domain.SymbolEntity
                    {
                        Name = s.Name,
                        TradeAllowed = s.IsTradeAllowed,
                        BaseCurrency = s.BaseCurrencyCode,
                        CounterCurrency = s.CounterCurrencyCode,
                        Digits = s.Digits,
                        LotSize = s.LotSize,
                        MinTradeVolume = s.MinTradeVolume,
                        MaxTradeVolume = s.MaxTradeVolume,
                        TradeVolumeStep = s.TradeVolumeStep,
                    }));
                return Any.Pack(response);
            }
        }
    }
}
