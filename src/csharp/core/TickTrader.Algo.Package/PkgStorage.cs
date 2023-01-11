using TickTrader.Algo.Async;
using TickTrader.Algo.Domain.ServerControl;
using TickTrader.Algo.Domain;
using System.Threading.Tasks;

namespace TickTrader.Algo.Package
{
    public interface IPkgStorage
    {
        IEventSource<PackageUpdate> OnPkgUpdated { get; }

        IEventSource<PackageStateUpdate> OnPkgStateUpdated { get; }


        Task<string> UploadPackage(UploadPackageRequest request, string pkgFilePath);

        Task RemovePackage(RemovePackageRequest request);
    }
}
