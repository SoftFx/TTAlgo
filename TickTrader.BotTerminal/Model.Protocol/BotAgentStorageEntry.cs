using System.Runtime.Serialization;

namespace TickTrader.BotTerminal
{
    [DataContract(Namespace = "")]
    internal class BotAgentStorageEntry
    {
        [DataMember]
        public string Name { get; private set; }

        [DataMember]
        public string Login { get; set; }

        [DataMember]
        public string Password { get; set; }

        public bool HasPassword => Password != null;

        [DataMember]
        public string ServerAddress { get; set; }

        [DataMember]
        public int Port { get; set; }

        [DataMember]
        public bool Connect { get; set; }


        public BotAgentStorageEntry(string name)
        {
            Name = name;
        }

        public BotAgentStorageEntry(string name, string login, string password, string server, int port)
        {
            Name = name;
            Login = login;
            Password = password;
            ServerAddress = server;
            Port = port;
        }


        public BotAgentStorageEntry Clone()
        {
            return new BotAgentStorageEntry(Name, Login, Password, ServerAddress, Port) { Connect = Connect };
        }

        public ProtocolClientSettings ToClientSettings()
        {
            return new ProtocolClientSettings { Login = Login, Password = Password, Port = Port, ServerAddress = ServerAddress };
        }

        // Compatibility with previous versions
        public void PatchName()
        {
            Name = ServerAddress;
        }
    }
}
