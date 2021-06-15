using System.Collections.Generic;
using System.Threading.Tasks;
using TickTrader.Algo.Async.Actors;
using TickTrader.Algo.Core.Lib;
using TickTrader.Algo.Package;

namespace TickTrader.Algo.Server
{
    public class RuntimeManager
    {
        private static readonly IAlgoLogger _logger = AlgoLoggerFactory.GetLogger<RuntimeManager>();

        private readonly IActorRef _impl = ActorSystem.SpawnLocal<Impl>();
        private readonly PackageStorage _pkgStorage;


        public RuntimeManager(PackageStorage pkgStorage)
        {
            _pkgStorage = pkgStorage;
            pkgStorage.PackageVersionUpdated.Subscribe(update => _impl.Tell(update));
        }


        //public async Task<RuntimeModel> GetPackageRuntime(string pkgId)
        //{
        //    var taskSrc = await _impl.Call(a => a.GetOrCreatePackageRuntime(pkgId));
        //    if (taskSrc != null)
        //    {
        //        return await taskSrc.Task;
        //    }
        //    var pkgRef = await _pkgStorage.GetPackageRef(pkgId);
        //    return await _impl.Call(a => a.CreatePackageRuntime(_server, pkgRef));
        //}


        private class Impl : Actor
        {
            private readonly Dictionary<string, RuntimeModel2> _runtimeMap = new Dictionary<string, RuntimeModel2>();
            private readonly Dictionary<string, string> _pkgRuntimeMap = new Dictionary<string, string>();


            public Impl()
            {
                Receive<PackageVersionUpdate>(HandlePackageVersionUpdate);
            }


            private void HandlePackageVersionUpdate(PackageVersionUpdate update)
            {
                var pkgId = update.PackageId;
                var pkgRefId = update.LatestPkgRefId;

                if (_pkgRuntimeMap.TryGetValue(pkgId, out var runtimeId))
                {
                    //_runtimeMap[runtimeId].SetShutdown();
                }

                if (string.IsNullOrEmpty(pkgRefId))
                    return;

                runtimeId = pkgRefId.Replace('/', '-');

                _pkgRuntimeMap[pkgId] = runtimeId;
                _runtimeMap[runtimeId] = new RuntimeModel2(runtimeId, pkgRefId);
            }

            //public RuntimeModel CreatePackageRuntime(AlgoServer server, AlgoPackageRef pkgRef)
            //{
            //    var pkgId = pkgRef.PackageId;
            //    if (!_pkgRuntimeMap.TryGetValue(pkgId, out var taskSrc))
            //    {
            //        taskSrc = new TaskCompletionSource<RuntimeModel>();
            //        _pkgRuntimeMap[pkgId] = taskSrc;
            //    }
            //    var runtimeId = pkgRef.Id;
            //    var runtime = new RuntimeModel(server, runtimeId, pkgRef);
            //    _runtimeMap[runtimeId] = runtime;
            //    taskSrc.SetResult(runtime);
            //    return runtime;
            //}

            //public RuntimeModel GetRuntime(string runtimeId)
            //{
            //    _runtimeMap.TryGetValue(runtimeId, out var runtime);
            //    return runtime;
            //}

            //public void OnRuntimeStopped(string runtimeId)
            //{
            //    _runtimeMap.Remove(runtimeId);
            //}
        }
    }
}
