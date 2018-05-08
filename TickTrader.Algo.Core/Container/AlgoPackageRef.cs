using System;
using System.Collections.Generic;
using System.Linq;

namespace TickTrader.Algo.Core.Metadata
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
        private int _refCount;


        public RepositoryLocation Location { get; }

        public string Name { get; }

        public DateTime Created { get; }

        public PluginContainer Container { get; }

        public bool IsValid => Container != null;

        public bool IsLocked => _refCount > 0;

        public bool IsObsolete { get; private set; }


        public event Action<AlgoPackageRef> IsLockedChanged;


        public AlgoPackageRef(RepositoryLocation location, string name, DateTime created, PluginContainer container)
        {
            Location = location;
            Name = name;
            Created = created;
            Container = container;
            IsObsolete = false;
        }

        public IEnumerable<AlgoPluginRef> GetPluginRefs()
        {
            return Container?.Plugins ?? Enumerable.Empty<AlgoPluginRef>();
        }

        public AlgoPluginRef GetPluginRef(string id)
        {
            return Container?.Plugins.FirstOrDefault(pr => pr.Id == id);
        }

        public void IncrementRef()
        {
            if (IsObsolete)
                throw new AlgoException($"Package {Name} at {Location} is obsolete");

            _refCount++;
            if (_refCount == 1)
                IsLockedChanged?.Invoke(this);
        }

        public void DecrementRef()
        {
            _refCount--;
            if (_refCount == 0)
            {
                IsLockedChanged?.Invoke(this);
                if (IsObsolete)
                    Dispose();
            }
        }

        public void Dispose()
        {
            Container?.Dispose();
        }

        public void SetObsolete()
        {
            if (IsObsolete)
                throw new AlgoException($"Package {Name} at {Location} already marked obsolete");

            IsObsolete = true;
            if (!IsLocked)
                Dispose();
        }
    }
}
