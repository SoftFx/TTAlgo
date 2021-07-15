using System.Collections.Generic;
using System.Reflection;

namespace TickTrader.Algo.Server
{
    public class PkgStorageSettings
    {
        public string UploadLocationId { get; set; }

        public List<RepositoryLocation> Locations { get; } = new List<RepositoryLocation>();

        public List<Assembly> Assemblies { get; } = new List<Assembly>();


        public void AddLocation(string locationId, string path) => Locations.Add(new RepositoryLocation(locationId, path));
    }

    public class RepositoryLocation
    {
        public string LocationId { get; }

        public string Path { get; }


        public RepositoryLocation(string locationId, string path)
        {
            LocationId = locationId;
            Path = path;
        }
    }
}
