using System;
using System.Threading;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Core.Repository
{
    public class AlgoPackageRef : IDisposable
    {
        private int _refCount;


        public string Name => PackageInfo.Key.Name;

        public PackageIdentity Identity { get; private set; }

        public PackageInfo PackageInfo { get; private set; }

        public bool IsValid => ActiveRuntime != null;

        public bool IsLocked => true;

        public bool IsObsolete { get; private set; }


        internal RuntimeModel ActiveRuntime { get; private set; }


        public event Action<AlgoPackageRef> IsLockedChanged;
        public event Action<AlgoPackageRef> IsObsoleteChanged;


        public AlgoPackageRef(PackageInfo packageInfo)
        {
            Identity = packageInfo.Identity;
            PackageInfo = packageInfo;
            IsObsolete = false;
        }


        public void IncrementRef()
        {
            //if (IsObsolete)
            //    throw new AlgoException($"Package {Name} at {Location} is obsolete");

            if (Interlocked.Increment(ref _refCount) == 1)
                OnLockedChanged();
        }

        public void DecrementRef()
        {
            _refCount--;
            if (Interlocked.Decrement(ref _refCount) == 0)
            {
                if (IsObsolete)
                    Dispose();

                OnLockedChanged();
            }
        }

        public void SetObsolete()
        {
            //if (IsObsolete)
            //    throw new AlgoException($"Algo Package {Name} at {Location} already marked obsolete");

            IsObsolete = true;
            if (!IsLocked)
                Dispose();

            OnObsoleteChanged();
        }


        internal void Dispose()
        {
        }

        internal void Update(RuntimeModel activeRuntime, PackageInfo packageInfo)
        {
            ActiveRuntime = activeRuntime;
            PackageInfo = packageInfo;
        }


        protected void OnLockedChanged()
        {
            IsLockedChanged?.Invoke(this);
        }

        protected void OnObsoleteChanged()
        {
            IsObsoleteChanged?.Invoke(this);
        }


        void IDisposable.Dispose()
        {
            Dispose();
        }
    }
}
