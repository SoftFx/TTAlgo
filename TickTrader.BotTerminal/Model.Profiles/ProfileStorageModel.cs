using System;
using System.Runtime.Serialization;

namespace TickTrader.BotTerminal
{
    [DataContract(Namespace = "", Name = "Profile")]
    internal class ProfileStorageModel : StorageModelBase<ProfileStorageModel>
    {
        [DataMember(Name = "Settings")]
        private SettingsModel _settings;


        public SettingsModel Settings
        {
            get
            {
                if (_settings == null)
                {
                    _settings = new SettingsModel();
                }

                return _settings;
            }
        }


        public ProfileStorageModel()
        {
            _settings = new SettingsModel();
        }


        public override ProfileStorageModel Clone()
        {
            return new ProfileStorageModel()
            {
                _settings = Settings.Clone()
            };
        }
    }
}
