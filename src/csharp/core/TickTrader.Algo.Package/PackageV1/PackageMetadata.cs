using System.Runtime.Serialization;

namespace TickTrader.Algo.Package.V1
{
    [DataContract(Name = "PackageMetadata", Namespace = "")]
    public class PackageMetadata
    {
        [DataMember]
        public string MainBinaryFile { get; set; }

        [DataMember]
        public string Runtime { get; set; }

        [DataMember]
        public string IDE { get; set; }

        [DataMember]
        public string ProjectFilePath { get; set; }

        [DataMember]
        public string Workspace { get; set; }
    }
}