using System;
using System.Threading;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Package
{
    public class AlgoPackageRef : IDisposable
    {
        private int _refCount;


        public string Id { get; }

        public string PackageId => PackageInfo.PackageId;

        public PackageIdentity Identity => PackageInfo.Identity;

        public PackageInfo PackageInfo { get; private set; }

        public byte[] PackageBytes { get; private set; }

        public bool IsLocked => _refCount > 0;

        public bool IsObsolete { get; private set; }


        public event Action<AlgoPackageRef> IsLockedChanged;
        public event Action<AlgoPackageRef> IsObsoleteChanged;


        public AlgoPackageRef(string id, PackageInfo packageInfo, byte[] pkgBytes)
        {
            Id = id;
            PackageInfo = packageInfo;
            PackageBytes = pkgBytes;
            IsObsolete = false;
        }


        internal void IncrementRef()
        {
            if (Interlocked.Increment(ref _refCount) == 1)
                OnLockedChanged();
        }

        internal void DecrementRef()
        {
            if (Interlocked.Decrement(ref _refCount) == 0)
            {
                if (IsObsolete)
                    Dispose();

                OnLockedChanged();
            }
        }

        internal void SetObsolete()
        {
            IsObsolete = true;
            if (!IsLocked)
                Dispose();

            OnObsoleteChanged();
        }

        internal void Dispose()
        {
            PackageBytes = null;
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
