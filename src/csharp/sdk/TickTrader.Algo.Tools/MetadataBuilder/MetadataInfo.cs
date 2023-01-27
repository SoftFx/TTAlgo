using System.Collections.Generic;
using System.Runtime.Serialization;

namespace TickTrader.Algo.Tools.MetadataBuilder
{
    [DataContract]
    public class MetadataInfo
    {
        public const string DateTimeFormat = "dd.MM.yyyy hh:mm:ss";


        [DataMember]
        public string Source { get; set; }

        [DataMember]
        public string ArtifactName { get; set; }

        [DataMember]
        public string Author { get; set; }

        [DataMember]
        public string ApiVersion { get; set; }

        [DataMember]
        public string BuildDate { get; set; }

        [DataMember]
        public long PackageSize { get; set; }

        [DataMember]
        public List<PluginsInfo> Plugins { get; set; } = new List<PluginsInfo>();
    }


    [DataContract]
    public class PluginsInfo
    {
        [DataMember]
        public string DisplayName { get; set; }

        [DataMember]
        public string Version { get; set; }

        [DataMember]
        public string Copyright { get; set; }

        [DataMember]
        public string Description { get; set; }

        [DataMember]
        public string Category { get; set; }
    }
}
