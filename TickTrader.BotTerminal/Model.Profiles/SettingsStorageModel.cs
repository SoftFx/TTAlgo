using System.Runtime.Serialization;

namespace TickTrader.BotTerminal
{
    [DataContract(Namespace = "", Name = "Preferences")]
    internal class SettingsStorageModel : StorageModelBase<SettingsStorageModel>
    {
        [DataMember]
        public string Theme { get; set; }

        [DataMember]
        public bool EnableSounds { get; set; }

        [DataMember]
        public bool EnableNotifications { get; set; }

        [DataMember]
        public bool RestartBotsOnStartup { get; set; }


        public SettingsStorageModel()
        {
            EnableSounds = true;
        }


        public override SettingsStorageModel Clone()
        {
            return new SettingsStorageModel
            {
                Theme = Theme,
                EnableSounds = EnableSounds,
                EnableNotifications = EnableNotifications,
                RestartBotsOnStartup = RestartBotsOnStartup,
            };
        }
    }
}
