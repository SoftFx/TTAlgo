using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace TickTrader.BotTerminal
{
    [DataContract(Namespace = "BotTerminal.Profile.v2", Name = "Profile")]
    internal class ProfileStorageModel : StorageModelBase<ProfileStorageModel>
    {
        [DataMember]
        public string SelectedChart { get; set; }

        [DataMember]
        public List<ChartStorageEntry> Charts { get; set; }

        [DataMember]
        public List<TradeBotStorageEntry> Bots { get; set; }

        [DataMember]
        public string Layout { get; set; }

        [DataMember]
        public List<ColumnStateStorageEntry> ColumnsShow { get; set; }

        public ProfileStorageModel()
        {}


        public override ProfileStorageModel Clone()
        {
            return new ProfileStorageModel()
            {
                SelectedChart = SelectedChart,
                Charts = Charts != null ? new List<ChartStorageEntry>(Charts.Select(c => c.Clone())) : null,
                Bots = Bots != null ? new List<TradeBotStorageEntry>(Bots.Select(c => c.Clone())) : null,
                ColumnsShow = ColumnsShow != null ? new List<ColumnStateStorageEntry>(ColumnsShow) : new List<ColumnStateStorageEntry>(),
                Layout = Layout,
            };
        }
    }
}
