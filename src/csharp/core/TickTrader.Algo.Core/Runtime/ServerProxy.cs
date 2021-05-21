using System.Threading.Tasks;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Rpc;

namespace TickTrader.Algo.Core
{
    public interface IServerProxy
    {
        Task<bool> AttachRuntime(string runtimeId);

        Task<RuntimeConfig> GetRuntimeConfig();

        Task<string> GetPackagePath(string name, int location);

        Task<ExecutorConfig> GetExecutorConfig(string executorId);

        Task<IAccountProxy> GetAccount(string accountId);
    }


    public class RemoteServerProxy : IServerProxy
    {
        private readonly RpcSession _session;


        public RemoteServerProxy(RpcSession session)
        {
            _session = session;
        }


        public async Task<bool> AttachRuntime(string runtimeId)
        {
            var context = new RpcResponseTaskContext<AttachRuntimeResponse>(RpcHandler.SingleReponseHandler);
            _session.Ask(RpcMessage.Request(new AttachRuntimeRequest { Id = runtimeId }), context);
            return (await context.TaskSrc.Task).Success;
        }

        public Task<RuntimeConfig> GetRuntimeConfig()
        {
            var context = new RpcResponseTaskContext<RuntimeConfig>(RpcHandler.SingleReponseHandler);
            _session.Ask(RpcMessage.Request(new RuntimeConfigRequest()), context);
            return context.TaskSrc.Task;
        }

        public async Task<string> GetPackagePath(string name, int location)
        {
            var request = new PackagePathRequest { Name = name, Location = location };
            var context = new RpcResponseTaskContext<PackagePathResponse>(RpcHandler.SingleReponseHandler);
            _session.Ask(RpcMessage.Request(request), context);
            return (await context.TaskSrc.Task).Path;
        }

        public Task<ExecutorConfig> GetExecutorConfig(string executorId)
        {
            var context = new RpcResponseTaskContext<ExecutorConfig>(RpcHandler.SingleReponseHandler);
            _session.Ask(RpcMessage.Request(executorId, new ExecutorConfigRequest()), context);
            return context.TaskSrc.Task;
        }

        public Task<IAccountProxy> GetAccount(string accountId)
        {
            return Task.FromResult<IAccountProxy>(new RemoteAccountProxy(accountId));
        }
    }
}
