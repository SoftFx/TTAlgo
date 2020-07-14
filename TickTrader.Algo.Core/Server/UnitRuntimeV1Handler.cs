using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.WellKnownTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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
            var context = new RpcResponseTaskContext<CurrencyListResponse>(SingleReponseHandler);
            _session.Ask(RpcMessage.Request(new CurrencyListRequest()), context);
            var res = context.TaskSrc.Task.GetAwaiter().GetResult();
            return res.Currencies.Select(c => new CurrencyEntity(c));
        }

        IEnumerable<SymbolInfo> IPluginMetadata.GetSymbolMetadata()
        {
            var context = new RpcResponseTaskContext<SymbolListResponse>(SingleReponseHandler);
            _session.Ask(RpcMessage.Request(new SymbolListRequest()), context);
            var res = context.TaskSrc.Task.GetAwaiter().GetResult();
            return res.Symbols;
        }

        internal AccountInfo GetAccountInfo()
        {
            var context = new RpcResponseTaskContext<AccountInfoResponse>(SingleReponseHandler);
            _session.Ask(RpcMessage.Request(new AccountInfoRequest()), context);
            var res = context.TaskSrc.Task.GetAwaiter().GetResult();
            return res.Account;
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


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool TryGetError(Any payload, out Exception ex)
        {
            ex = null;
            if (payload.Is(RpcError.Descriptor))
            {
                var error = payload.Unpack<RpcError>();
                ex = new Exception(error.Message);
                return true;
            }
            return false;
        }

        private bool SingleReponseHandler<T>(TaskCompletionSource<T> taskSrc, Any payload) where T : IMessage, new()
        {
            if (TryGetError(payload, out var ex))
            {
                taskSrc.TrySetException(ex);
            }
            else
            {
                var response = payload.Unpack<T>();
                taskSrc.TrySetResult(response);
            }

            return true;
        }

        private bool ListReponseHandler<T>(IObserver<RepeatedField<T>> observer, Any payload) where T : IMessage, new()
        {
            if (TryGetError(payload, out var ex))
            {
                observer.OnError(ex);
            }
            else
            {
                var response = payload.Unpack<T>();
                //observer.OnNext(response.Items);
                //if (!response.IsFinal)
                //    return false;

                observer.OnCompleted();
            }

            return true;
        }
    }
}
