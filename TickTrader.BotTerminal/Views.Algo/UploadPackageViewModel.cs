using Machinarium.Var;
using System.Collections;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TickTrader.Algo.Core.Repository;

namespace TickTrader.BotTerminal
{
    internal sealed class UploadPackageViewModel : BaseloadPackageViewModel
    {
        public override IProperty<string> SourcePackageName => LocalPackageName;

        public override IProperty<string> TargetPackageName => AlgoServerPackageName;

        protected override ICollection SourceCollection => _localPackages;


        public UploadPackageViewModel(AlgoAgentViewModel algoServer, string packageId = null) : base(algoServer, LoadPackageMode.Upload)
        {
            SourcePackageCollectionView.SortDescriptions.Add(new SortDescription
            {
                Direction = ListSortDirection.Ascending
            });

            if (packageId != null)
                PackageHelper.UnpackPackageId(packageId, out _, out packageId);

            SetStartLocation(packageId);
        }

        protected override async Task RunLoadPackageProgress()
        {
            _progressModel.SetMessage($"Uploading Algo Package {LocalPackageName.DisplayValue} to {SelectedAlgoServer.Name}");

            var selectedPackagePath = FullPackagePath(LocalPackageName.Value);
            var progressListener = new FileProgressListenerAdapter(_progressModel, new FileInfo(selectedPackagePath).Length);

            await SelectedAlgoServer.Model.UploadPackage(AlgoServerPackageName.Value, selectedPackagePath, progressListener);
        }


        protected override string GetTargetPackageName(string packageName) => GetAlgoServerPackageName(packageName);

        protected override string GetSourcePackageName(string packageName) => GetLocalPackageName(packageName);

        protected override string GetFirstSourcePackageName() => _localPackages.FirstOrDefault();
    }
}
