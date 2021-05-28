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
            };
        }
    }
}
