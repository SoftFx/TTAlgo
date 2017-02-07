using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace TickTrader.Algo.Core
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
