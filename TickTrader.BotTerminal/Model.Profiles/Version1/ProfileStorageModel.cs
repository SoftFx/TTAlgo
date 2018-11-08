using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace TickTrader.BotTerminal.Model.Profiles.Version1
{
    [DataContract(Namespace = "", Name = "Profile")]
    internal class ProfileStorageModel : StorageModelBase<ProfileStorageModel>
    {
        [DataMember]
        public string SelectedChart { get; set; }

        [DataMember]
        public List<ChartStorageEntry> Charts { get; set; }


        public ProfileStorageModel()
        {
        }


        public override ProfileStorageModel Clone()
        {
            return new ProfileStorageModel()
            {
                SelectedChart = SelectedChart,
                Charts = Charts != null ? new List<ChartStorageEntry>(Charts.Select(c => c.Clone())) : null,
            };
        }
    }
}
