using System.Threading.Tasks;
using TickTrader.Algo.Async.Actors;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Server.Persistence;

namespace TickTrader.Algo.Server
{
    public interface IPluginHost
    {
        Task UpdateRunningState(string pluginId, bool isRunning);

        Task UpdateSavedState(PluginSavedState savedState);

        void OnPluginUpdated(PluginModelUpdate update);

        void OnPluginStateUpdated(PluginStateUpdate update);

        void OnPluginAlert(string pluginId, PluginLogRecord record);

        void OnGlobalAlert(string msg);

        Task<string> GetPkgRuntimeId(string pkgId);

        Task<IActorRef> GetRuntime(string runtimeId);

        ExecutorConfig CreateExecutorConfig(string pluginId, string accId, PluginConfig pluginConfig);
    }
}
