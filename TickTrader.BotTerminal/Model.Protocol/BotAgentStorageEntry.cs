using System.Runtime.Serialization;

namespace TickTrader.BotTerminal
{
    [DataContract(Namespace = "")]
    internal class BotAgentStorageEntry
    {
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
        public string CertificateName { get; set; }

        [DataMember]
        public bool Connect { get; set; }


        public BotAgentStorageEntry()
        {
        }

        public BotAgentStorageEntry(string login, string password, string server, int port, string certificate)
        {
            Login = login;
            Password = password;
            ServerAddress = server;
            Port = port;
            CertificateName = certificate;
        }


        public BotAgentStorageEntry Clone()
        {
            return new BotAgentStorageEntry(Login, Password, ServerAddress, Port, CertificateName) { Connect = Connect };
        }

        public ProtocolClientSettings ToClientSettings()
        {
            return new ProtocolClientSettings { Login = Login, Password = Password, Port = Port, ServerAddress = ServerAddress, ServerCertificateName = CertificateName };
        }
    }
}
