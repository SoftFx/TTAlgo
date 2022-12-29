using System.Threading.Tasks;
using TickTrader.Algo.Async;
using TickTrader.Algo.Async.Actors;
using TickTrader.Algo.CoreV1;
using TickTrader.Algo.Domain;
using TickTrader.Algo.Package;
using TickTrader.Algo.Server.PublicAPI.Converters;
using PublicApi = TickTrader.Algo.Server.PublicAPI;

namespace TickTrader.Algo.PkgStorage
{
    public interface IPkgStorage
    {
        IEventSource<PublicApi.PackageUpdate> OnPkgUpdated { get; }

        IEventSource<PublicApi.PackageStateUpdate> OnPkgStateUpdated { get; }


        Task Init(PkgStorageSettings settings);

        Task Deinit();

        Task WhenLoaded();

        void UploadPackage(PublicApi.UploadPackageRequest request, string pkgFilePath);

        void RemovePackage(PublicApi.RemovePackageRequest request);
    }


    public class PkgStoragePublic : IPkgStorage
    {
        private readonly ChannelEventSource<PublicApi.PackageUpdate> _pkgUpdateEventSrc = new ChannelEventSource<PublicApi.PackageUpdate>();
        private readonly ChannelEventSource<PublicApi.PackageStateUpdate> _pkgStateUpdateEventSrc = new ChannelEventSource<PublicApi.PackageStateUpdate>();

        private IActorRef _eventBus;
        private PackageStorage _pkgStorage;


        public IEventSource<PublicApi.PackageUpdate> OnPkgUpdated => _pkgUpdateEventSrc;

        public IEventSource<PublicApi.PackageStateUpdate> OnPkgStateUpdated => _pkgStateUpdateEventSrc;


        public async Task Init(PkgStorageSettings settings)
        {
            _eventBus = EventBus.Create(this);
            _pkgStorage = new PackageStorage(_eventBus);

            await _pkgStorage.Start(settings);
        }

        public async Task Deinit()
        {
            await _pkgStorage.Stop();
            await ActorSystem.StopActor(_eventBus);
        }

        public Task WhenLoaded() => _pkgStorage.WhenLoaded();

        public void UploadPackage(PublicApi.UploadPackageRequest request, string pkgFilePath) => _pkgStorage.UploadPackage(request.ToServer(), pkgFilePath);

        public void RemovePackage(PublicApi.RemovePackageRequest request) => _pkgStorage.RemovePackage(request.ToServer());


        static PkgStoragePublic()
        {
            PackageExplorer.Init<PackageV1Explorer>();
#if !NETSTANDARD
            PackageLoadContext.Init(Isolation.PackageLoadContextProvider.Create);
#endif
        }


        private class EventBus : Actor
        {
            private readonly PkgStoragePublic _parent;


            private EventBus(PkgStoragePublic parent)
            {
                _parent = parent;

                Receive<PackageUpdate>(OnPackageUpdate);
                Receive<PackageStateUpdate>(OnPackageStateUpdate);
            }

            public static IActorRef Create(PkgStoragePublic parent)
            {
                return ActorSystem.SpawnLocal(() => new EventBus(parent), $"{nameof(PkgStoragePublic)}+{nameof(EventBus)}");
            }


            private void OnPackageUpdate(PackageUpdate update) => _parent._pkgUpdateEventSrc.Send(update.ToApi());

            private void OnPackageStateUpdate(PackageStateUpdate update) => _parent._pkgStateUpdateEventSrc.Send(update.ToApi());
        }
    }
}
