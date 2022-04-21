using Machinarium.Var;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;

namespace TickTrader.BotTerminal
{
    internal sealed class DownloadPackageViewModel : BaseloadPackageViewModel
    {
        public override IProperty<string> SourcePackageName => AlgoServerPackageName;

        public override IProperty<string> TargetPackageName => LocalPackageName;

        protected override ICollection SourceCollection => SelectedAlgoServer.PackageList;


        public DownloadPackageViewModel(AlgoAgentViewModel algoServer, string packageId) : base(algoServer, LoadPackageMode.Download)
        {
            if (packageId != null)
                packageId = SelectedAlgoServer.PackageList.FirstOrDefault(u => u.Key == packageId)?.FileName;

            SetStartLocation(packageId);
        }

        protected override async Task RunLoadPackageProgress()
        {
            _progressModel.SetMessage($"Downloading Algo Package {AlgoServerPackageName.DisplayValue} from {SelectedAlgoServer.Name} to {SelectedFolder}");

            var selectedAlgoPackage = SelectedAlgoServer.PackageList.FirstOrDefault(u => u.FileName == AlgoServerPackageName.Value);

            if (selectedAlgoPackage != null)
            {
                var progressListener = new FileProgressListenerAdapter(_progressModel, selectedAlgoPackage.Identity.Size);

                await SelectedAlgoServer.Model.DownloadPackage(selectedAlgoPackage.Key, FullPackagePath(TargetPackageName.Value), progressListener);
            }
        }


        protected override string GetTargetPackageName(string packageName) => GetLocalPackageName(packageName);

        protected override string GetSourcePackageName(string packageName) => GetAlgoServerPackageName(packageName);

        protected override string GetFirstSourcePackageName() => SelectedAlgoServer.PackageList.FirstOrDefault()?.FileName;
    }
}
