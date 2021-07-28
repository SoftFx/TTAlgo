using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Package
{
    public enum PkgUpdateAction { Upsert, Remove }


    public sealed class PackageFileUpdate
    {
        public string PkgId { get; }

        public PkgUpdateAction Action { get; }

        public PackageInfo PkgInfo { get; }

        public byte[] PkgBytes { get; }


        public PackageFileUpdate(string pkgId, PkgUpdateAction action, PackageInfo pkgInfo = null, byte[] pkgBytes = null)
        {
            PkgId = pkgId;
            Action = action;
            PkgInfo = pkgInfo;
            PkgBytes = pkgBytes;
        }
    }
}
