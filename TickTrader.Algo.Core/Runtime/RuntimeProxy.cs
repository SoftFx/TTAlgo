using System.Threading.Tasks;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Rpc;

namespace TickTrader.Algo.Core
{
    public interface IRuntimeProxy
    {
        Task Launch();

        Task Stop();

        Task StartExecutor(string executorId);

        Task StopExecutor(string executorId);
    }


    public class RuntimeProxy : IRuntimeProxy
    {
        private readonly RpcSession _session;


        public RuntimeProxy(RpcSession session)
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
    }
}
