using System.Runtime.Serialization;

namespace TickTrader.BotTerminal
{
    [DataContract(Namespace = "")]
    public class SettingsModel
    {
        [DataMember]
        public bool EnableSounds { get; set; }

        [DataMember]
        public bool EnableNotifications { get; set; }


        public SettingsModel()
        {
        }


        public SettingsModel Clone()
        {
            return new SettingsModel
            {
                EnableSounds = EnableSounds,
                EnableNotifications = EnableNotifications,
            };
        }
    }
}
