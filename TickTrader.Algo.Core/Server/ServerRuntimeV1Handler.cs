using Google.Protobuf.WellKnownTypes;
using System;
using System.Linq;
using System.Threading;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Rpc;

namespace TickTrader.Algo.Core
{
    internal class ServerRuntimeV1Handler : IRpcHandler
    {
        private readonly AlgoServer _server;
        private PluginExecutor _executor;
        private RpcSession _session;


        public ServerRuntimeV1Handler(AlgoServer server)
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
                if (_server.TryGetExecutor(request.Id, out var executor))
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
                .Select(c => c.Info));
            return Any.Pack(response);
        }

        private Any SymbolListRequestHandler()
        {
            var response = new SymbolListResponse();
            response.Symbols.Add(_executor.Metadata.GetSymbolMetadata());
            return Any.Pack(response);
        }
    }
}
