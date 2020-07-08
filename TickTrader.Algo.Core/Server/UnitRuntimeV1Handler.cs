using Google.Protobuf.WellKnownTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Rpc;

namespace TickTrader.Algo.Core
{
    internal class UnitRuntimeV1Handler : IRpcHandler, IPluginMetadata
    {
        private readonly PluginExecutorCore _executorCore;
        private RpcSession _session;


        public UnitRuntimeV1Handler(PluginExecutorCore executorCore)
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
