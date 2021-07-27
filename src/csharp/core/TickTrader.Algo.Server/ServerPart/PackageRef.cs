using System.Threading;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Server
{
    internal class PackageRef
    {
        private readonly SynchronizationContext _syncContext;
        private int _refCnt;


        public string Id { get; }

        public string PkgId => PkgInfo.PackageId;

        public PackageInfo PkgInfo { get; private set; }

        public byte[] PkgBytes { get; private set; }

        public bool IsLocked => _refCnt > 0;

        public bool IsObsolete { get; private set; }

        public string FilePath => PkgInfo.Identity.FilePath;


        public PackageRef(string id, PackageInfo packageInfo, byte[] pkgBytes)
        {
            Id = id;
            PkgInfo = packageInfo;
            PkgBytes = pkgBytes;
            IsObsolete = false;

            _syncContext = SynchronizationContext.Current ?? throw Errors.MissingSynchronizationContext();
        }


        internal bool IncrementRef()
        {
            CheckContext();

            _refCnt++;
            return _refCnt == 1;
        }

        internal bool DecrementRef()
        {
            CheckContext();

            _refCnt--;
            var gotUnlocked = _refCnt == 0;
            if (gotUnlocked && IsObsolete)
                Dispose();

            return gotUnlocked;
        }

        internal void SetObsolete()
        {
            CheckContext();

            IsObsolete = true;
            if (!IsLocked)
                Dispose();
        }


        private void Dispose()
        {
            PkgBytes = null;
        }

        private void CheckContext()
        {
            if (SynchronizationContext.Current != _syncContext)
                throw Errors.SynchronizationContextIsDifferent();
        }
    }
}
