using System.Threading.Tasks;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Core
{
    public interface IRuntimeProxy
    {
        Task Start(StartRuntimeRequest request);

        Task Stop(StopRuntimeRequest request);

        Task StartExecutor(StartExecutorRequest request);

        Task StopExecutor(StopExecutorRequest request);
    }
}
