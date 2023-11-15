using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace TickTrader.BotTerminal
{
    [DataContract(Namespace = "", Name = "Preferences")]
    internal class PreferencesStorageModel : StorageModelBase<PreferencesStorageModel>
    {
        [DataMember]
        public string Theme { get; set; }

        [DataMember]
        public bool EnableSounds { get; set; }

        [DataMember]
        public bool EnableNotifications { get; set; }

        [DataMember]
        public bool RestartBotsOnStartup { get; set; }

        [DataMember]
        public List<DownloadSourceSettings> AppUpdateSources { get; set; } = new List<DownloadSourceSettings>();


        public PreferencesStorageModel()
        {
            EnableSounds = true;
        }


        public override PreferencesStorageModel Clone()
        {
            return new PreferencesStorageModel
            {
                Theme = Theme,
                EnableSounds = EnableSounds,
                EnableNotifications = EnableNotifications,
                RestartBotsOnStartup = RestartBotsOnStartup,
                AppUpdateSources = AppUpdateSources.Select(s => s.Clone()).ToList(),
            };
        }
    }


    [DataContract]
    internal class DownloadSourceSettings
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Uri { get; set; }


        public DownloadSourceSettings Clone()
        {
            return new DownloadSourceSettings
            {
                Name = Name,
                Uri = Uri,
            };
        }
    }
}
