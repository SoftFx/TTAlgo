using System;
using System.Collections.Generic;
using System.Linq;

namespace TickTrader.Algo.Core.Repository
{
    public enum RepositoryLocation
    {
        Embedded = 0,
        LocalRepository = 1,
        LocalExtensions = 2,
        CommonRepository = 3,
        CommonExtensions = 4,
    }


    public class AlgoPackageRef : IDisposable
    {
        private List<AlgoPluginRef> _plugins;


        public string Name { get; }

        public RepositoryLocation Location { get; }

        public DateTime CreatedUtc { get; }

        public virtual bool IsValid => true;

        public virtual bool IsLocked => true;

        public bool IsObsolete { get; private set; }


        public event Action<AlgoPackageRef> IsLockedChanged;


        protected AlgoPackageRef(string name, RepositoryLocation location, DateTime createdUtc)
        {
            Name = name;
            Location = location;
            CreatedUtc = createdUtc;
            IsObsolete = false;
        }

        public AlgoPackageRef(string name, RepositoryLocation location, DateTime createdUtc, IEnumerable<AlgoPluginRef> plugins)
            : this(name, location, createdUtc)
        {
            _plugins = plugins?.ToList();
        }


        public virtual IEnumerable<AlgoPluginRef> GetPluginRefs()
        {
            return _plugins ?? Enumerable.Empty<AlgoPluginRef>();
        }

        public virtual AlgoPluginRef GetPluginRef(string id)
        {
            return _plugins?.FirstOrDefault(pr => pr.Id == id);
        }

        public virtual void IncrementRef() { }

        public virtual void DecrementRef() { }

        public void SetObsolete()
        {
            if (IsObsolete)
                throw new AlgoException($"Package {Name} at {Location} already marked obsolete");

            IsObsolete = true;
            if (!IsLocked)
                Dispose();
        }


        internal virtual void Dispose() { }


        protected void OnLockedChanged()
        {
            IsLockedChanged?.Invoke(this);
        }


        void IDisposable.Dispose()
        {
            Dispose();
        }
    }


    public class IsolatedAlgoPackageRef : AlgoPackageRef
    {
        private int _refCount;


        public PluginContainer Container { get; }

        public override bool IsValid => Container != null;

        public override bool IsLocked => _refCount > 0;


        public IsolatedAlgoPackageRef(string name, RepositoryLocation location, DateTime createdUtc, PluginContainer container)
            : base(name, location, createdUtc)
        {
            Container = container;
        }

        public override IEnumerable<AlgoPluginRef> GetPluginRefs()
        {
            return Container?.Plugins ?? Enumerable.Empty<AlgoPluginRef>();
        }

        public override AlgoPluginRef GetPluginRef(string id)
        {
            return Container?.Plugins.FirstOrDefault(pr => pr.Id == id);
        }

        public override void IncrementRef()
        {
            if (IsObsolete)
                throw new AlgoException($"Package {Name} at {Location} is obsolete");

            _refCount++;
            if (_refCount == 1)
                OnLockedChanged();
        }

        public override void DecrementRef()
        {
            _refCount--;
            if (_refCount == 0)
            {
                OnLockedChanged();
                if (IsObsolete)
                    Dispose();
            }
        }


        internal override void Dispose()
        {
            Container?.Dispose();
        }
    }
}
