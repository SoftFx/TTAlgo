namespace TickTrader.Algo.Domain.ServerControl
{
    public partial class UploadPackageRequest
    {
        public UploadPackageRequest(string packageId, string filename)
            : this (packageId, filename, FileTransferSettings.Default)
        {
        }

        public UploadPackageRequest(string packageId, string filename, FileTransferSettings transferSettings)
        {
            PackageId = packageId;
            Filename = filename;
            TransferSettings = transferSettings;
        }


        public void Deconstruct(out string packageId, out string filename) => (packageId, filename) = (PackageId, Filename);
    }
}
