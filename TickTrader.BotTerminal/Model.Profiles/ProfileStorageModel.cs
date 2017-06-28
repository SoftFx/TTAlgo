using System;
using System.Runtime.Serialization;

namespace TickTrader.BotTerminal
{
    [DataContract(Namespace = "", Name = "Profile")]
    internal class ProfileStorageModel : StorageModelBase<ProfileStorageModel>
    {
        [DataMember(Name = "Settings")]
        private SettingsModel _settings;


        [DataMember]
        public string Server { get; set; }

        [DataMember]
        public string Login { get; set; }

        public SettingsModel Settings => _settings;


        public ProfileStorageModel()
        {
            _settings = new SettingsModel();
        }

        public ProfileStorageModel(string server, string login) : this()
        {
            Server = server;
            Login = login;
        }


        public override ProfileStorageModel Clone()
        {
            return new ProfileStorageModel(Server, Login)
            {
                _settings = Settings.Clone()
            };
        }
    }
}
