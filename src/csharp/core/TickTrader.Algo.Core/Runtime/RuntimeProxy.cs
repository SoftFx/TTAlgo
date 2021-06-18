using System.Threading.Tasks;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Core
{
    public interface IRuntimeProxy
    {
        Task Launch();

        Task Stop();

        Task StartExecutor(string executorId);

        Task StopExecutor(string executorId);

        Task<PackageInfo> GetPackageInfo();
    }
}
