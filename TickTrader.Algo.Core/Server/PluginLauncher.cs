using Google.Protobuf.WellKnownTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Rpc;
using TickTrader.Algo.Rpc.OverGrpc;

namespace TickTrader.Algo.Core
{
    public class PluginLauncher : CrossDomainObject, IRpcHost
    {
        private readonly RpcClient _client;


        public PluginExecutorCore Core { get; private set; }

        internal ExecutorCoreHandler Handler { get; private set; }


        public PluginLauncher()
        {
            _client = new RpcClient(new GrpcFactory(), this, new ProtocolSpec { MajorVerion = 1, MinorVerion = 0 });
        }


        public void Launch(string address, int port, string executorId)
        {
            ThreadPool.QueueUserWorkItem(async _ =>
            {
                try
                {
                    await _client.Connect(address, port);
                    await Handler.AttachPlugin(executorId);
                }
                catch (Exception) { }
            });
        }

        public PluginExecutorCore CreateExecutor(string pluginId)
        {
            Core = new PluginExecutorCore(pluginId);
            Handler = new ExecutorCoreHandler(Core);
            return Core;
        }

        public void ConfigureCore()
        {
            Core.Metadata = Handler;
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
                    return Handler;
            }
            return null;
        }

        #endregion IRpcHost implementation
    }

    internal class ExecutorCoreHandler : IRpcHandler, IPluginMetadata
    {
        private readonly PluginExecutorCore _executorCore;
        private RpcSession _session;


        public ExecutorCoreHandler(PluginExecutorCore executorCore)
        {
            _executorCore = executorCore;
        }


        public Task<bool> AttachPlugin(string executorId)
        {
            var context = new RpcResponseTaskContext<bool>(AttachPluginResponseHandler);
            _session.Ask(RpcMessage.Request(new AttachPluginRequest { Id = executorId }), context);
            return context.TaskSrc.Task;
        }


        IEnumerable<CurrencyEntity> IPluginMetadata.GetCurrencyMetadata()
        {
            var context = new RpcResponseTaskContext<CurrencyListResponse>(CurrencyListResponseHandler);
            _session.Ask(RpcMessage.Request(new CurrencyListRequest()), context);
            var res = context.TaskSrc.Task.GetAwaiter().GetResult();
            return res.Currencies.Select(c => new CurrencyEntity(c.Name, c.Digits));
        }

        IEnumerable<SymbolEntity> IPluginMetadata.GetSymbolMetadata()
        {
            var context = new RpcResponseTaskContext<SymbolListResponse>(SymbolListResponseHandler);
            _session.Ask(RpcMessage.Request(new SymbolListRequest()), context);
            var res = context.TaskSrc.Task.GetAwaiter().GetResult();
            return res.Symbols.Select(s => new SymbolEntity(s.Name)
            {
                IsTradeAllowed = s.TradeAllowed,
                BaseCurrencyCode = s.BaseCurrency,
                CounterCurrencyCode = s.CounterCurrency,
                Digits = s.Digits,
                LotSize = s.LotSize,
                MinAmount = s.MinTradeVolume,
                MaxAmount = s.MaxTradeVolume,
                AmountStep = s.TradeVolumeStep,
            });
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
            throw new NotImplementedException();
        }


        private bool AttachPluginResponseHandler(TaskCompletionSource<bool> taskSrc, Any payload)
        {
            if (payload.Is(RpcError.Descriptor))
            {
                var error = payload.Unpack<RpcError>();
                taskSrc.TrySetException(new Exception(error.Message));
                return true;
            }
            var response = payload.Unpack<AttachPluginResponse>();
            taskSrc.TrySetResult(response.Success);
            return true;
        }

        private bool CurrencyListResponseHandler(TaskCompletionSource<CurrencyListResponse> taskSrc, Any payload)
        {
            if (payload.Is(RpcError.Descriptor))
            {
                var error = payload.Unpack<RpcError>();
                taskSrc.TrySetException(new Exception(error.Message));
                return true;
            }
            var response = payload.Unpack<CurrencyListResponse>();
            taskSrc.TrySetResult(response);
            return true;
        }

        private bool SymbolListResponseHandler(TaskCompletionSource<SymbolListResponse> taskSrc, Any payload)
        {
            if (payload.Is(RpcError.Descriptor))
            {
                var error = payload.Unpack<RpcError>();
                taskSrc.TrySetException(new Exception(error.Message));
                return true;
            }
            var response = payload.Unpack<SymbolListResponse>();
            taskSrc.TrySetResult(response);
            return true;
        }
    }
}
