using System;
using System.Threading;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Package
{
    public class AlgoPackageRef : IDisposable
    {
        private byte[] _pkgBytes;
        private int _refCount;


        public string Id { get; }

        public string PackageId => PackageInfo.PackageId;

        public PackageIdentity Identity => PackageInfo.Identity;

        public PackageInfo PackageInfo { get; private set; }

        public bool IsLocked => _refCount > 0;

        public bool IsObsolete { get; private set; }


        public event Action<AlgoPackageRef> IsLockedChanged;
        public event Action<AlgoPackageRef> IsObsoleteChanged;


        public AlgoPackageRef(string id, PackageInfo packageInfo, byte[] pkgBytes)
        {
            Id = id;
            PackageInfo = packageInfo;
            _pkgBytes = pkgBytes;
            IsObsolete = false;
        }


        public void IncrementRef()
        {
            if (Interlocked.Increment(ref _refCount) == 1)
                OnLockedChanged();
        }

        public void DecrementRef()
        {
            if (Interlocked.Decrement(ref _refCount) == 0)
            {
                if (IsObsolete)
                    Dispose();

                OnLockedChanged();
            }
        }

        public void SetObsolete()
        {
            IsObsolete = true;
            if (!IsLocked)
                Dispose();

            OnObsoleteChanged();
        }


        internal void Dispose()
        {
            _pkgBytes = null;
        }


        private void OnLockedChanged()
        {
            IsLockedChanged?.Invoke(this);
        }

        private void OnObsoleteChanged()
        {
            IsObsoleteChanged?.Invoke(this);
        }


        void IDisposable.Dispose()
        {
            Dispose();
        }
    }
}
