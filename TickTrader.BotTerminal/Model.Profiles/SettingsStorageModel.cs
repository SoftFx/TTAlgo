using System.Runtime.Serialization;

namespace TickTrader.BotTerminal
{
    [DataContract(Namespace = "", Name = "Preferences")]
    internal class SettingsStorageModel : StorageModelBase<SettingsStorageModel>
    {
        [DataMember]
        public bool EnableSounds { get; set; }

        [DataMember]
        public bool EnableNotifications { get; set; }


        public SettingsStorageModel()
        {
            EnableSounds = true;
        }


        public override SettingsStorageModel Clone()
        {
            return new SettingsStorageModel
            {
                EnableSounds = EnableSounds,
                EnableNotifications = EnableNotifications,
            };
        }
    }
}
