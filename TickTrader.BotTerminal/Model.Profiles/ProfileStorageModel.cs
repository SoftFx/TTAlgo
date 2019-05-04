using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace TickTrader.BotTerminal
{
    [DataContract(Namespace = "BotTerminal.Profile.v2", Name = "Profile")]
    internal class ProfileStorageModel : StorageModelBase<ProfileStorageModel>
    {
        [DataMember(Name = "ViewModelStorages")]
        private List<ViewModelStorageEntry> _viewModelStorages;


        [DataMember]
        public string SelectedChart { get; set; }

        [DataMember]
        public List<ChartStorageEntry> Charts { get; set; }

        [DataMember]
        public List<TradeBotStorageEntry> Bots { get; set; }

        [DataMember]
        public string Layout { get; set; }


        public ProfileStorageModel() { }


        public override ProfileStorageModel Clone()
        {
            return new ProfileStorageModel()
            {
                SelectedChart = SelectedChart,
                Charts = Charts?.Select(c => c.Clone()).ToList(),
                Bots = Bots?.Select(b => b.Clone()).ToList(),
                Layout = Layout,
                _viewModelStorages = _viewModelStorages?.Select(s => s.Clone()).ToList(),
            };
        }

        public ViewModelStorageEntry GetViewModelStorage(string name)
        {
            if (_viewModelStorages == null)
                _viewModelStorages = new List<ViewModelStorageEntry>();

            var storage = _viewModelStorages.FirstOrDefault(c => c.Name == name);
            if (storage == null)
            {
                storage = new ViewModelStorageEntry(name);
                _viewModelStorages.Add(storage);
            }

            return storage;
        }
    }
}
