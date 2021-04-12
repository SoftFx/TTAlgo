namespace TickTrader.Algo.Domain.ServerControl
{
    public partial class DownloadPackageRequest
    {
        public DownloadPackageRequest(string packageId)
            : this(packageId, FileTransferSettings.Default)
        {
        }

        public DownloadPackageRequest(string packageId, FileTransferSettings transferSettings)
        {
            PackageId = packageId;
            TransferSettings = transferSettings;
        }
    }
}
