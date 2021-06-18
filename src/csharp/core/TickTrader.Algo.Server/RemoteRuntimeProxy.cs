using System.Threading.Tasks;
using TickTrader.Algo.Core;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Rpc;

namespace TickTrader.Algo.Server
{
    public class RemoteRuntimeProxy : IRuntimeProxy
    {
        private readonly RpcSession _session;


        public RemoteRuntimeProxy(RpcSession session)
        {
            _session = session;
        }


        public Task Launch()
        {
            var context = new RpcResponseTaskContext<VoidResponse>(RpcHandler.SingleReponseHandler);
            _session.Ask(RpcMessage.Request(new StartRuntimeRequest()), context);
            return context.TaskSrc.Task;
        }

        public Task Stop()
        {
            var context = new RpcResponseTaskContext<VoidResponse>(RpcHandler.SingleReponseHandler);
            _session.Ask(RpcMessage.Request(new StopRuntimeRequest()), context);
            return context.TaskSrc.Task;
        }

        public Task StartExecutor(string executorId)
        {
            var context = new RpcResponseTaskContext<VoidResponse>(RpcHandler.SingleReponseHandler);
            _session.Ask(RpcMessage.Request(new StartExecutorRequest { ExecutorId = executorId }), context);
            return context.TaskSrc.Task;
        }

        public Task StopExecutor(string executorId)
        {
            var context = new RpcResponseTaskContext<VoidResponse>(RpcHandler.SingleReponseHandler);
            _session.Ask(RpcMessage.Request(new StopExecutorRequest { ExecutorId = executorId }), context);
            return context.TaskSrc.Task;
        }

        public Task<PackageInfo> GetPackageInfo()
        {
            var context = new RpcResponseTaskContext<PackageInfo>(RpcHandler.SingleReponseHandler);
            _session.Ask(RpcMessage.Request(new PackageInfoRequest()), context);
            return context.TaskSrc.Task;
        }
    }
}
