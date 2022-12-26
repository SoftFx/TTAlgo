using System.Threading.Tasks;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Rpc;

namespace TickTrader.Algo.Runtime
{
    public class RemoteRuntimeProxy : IRuntimeProxy
    {
        private readonly RpcSession _session;


        public RemoteRuntimeProxy(RpcSession session)
        {
            _session = session;
        }


        public Task Start(StartRuntimeRequest request)
        {
            var context = new RpcResponseTaskContext<VoidResponse>(RpcHandler.SingleReponseHandler);
            _session.Ask(RpcMessage.Request(request), context);
            return context.TaskSrc.Task;
        }

        public Task Stop(StopRuntimeRequest request)
        {
            var context = new RpcResponseTaskContext<VoidResponse>(RpcHandler.SingleReponseHandler);
            _session.Ask(RpcMessage.Request(request), context);
            return context.TaskSrc.Task;
        }

        public Task StartExecutor(StartExecutorRequest request)
        {
            var context = new RpcResponseTaskContext<VoidResponse>(RpcHandler.SingleReponseHandler);
            _session.Ask(RpcMessage.Request(request), context);
            return context.TaskSrc.Task;
        }

        public Task StopExecutor(StopExecutorRequest request)
        {
            var context = new RpcResponseTaskContext<VoidResponse>(RpcHandler.SingleReponseHandler);
            _session.Ask(RpcMessage.Request(request), context);
            return context.TaskSrc.Task;
        }
    }
}
