using Google.Protobuf;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Server.Persistence
{
    internal class PluginSavedState
    {
        public string Id { get; set; }

        public string AccountId { get; set; }

        public bool IsRunning { get; set; }

        public string ConfigUri { get; set; }

        public string ConfigData { get; set; }


        public PluginSavedState Clone()
        {
            return new PluginSavedState
            {
                Id = Id,
                AccountId = AccountId,
                IsRunning = IsRunning,
                ConfigUri = ConfigUri,
                ConfigData = ConfigData,
            };
        }


        public PluginConfig UnpackConfig()
        {
            if (ConfigUri == PluginConfig.Descriptor.FullName)
                return PluginConfig.Parser.ParseFrom(ByteString.CopyFromUtf8(ConfigData));

            return null;
        }

        public void PackConfig(PluginConfig config)
        {
            ConfigUri = PluginConfig.Descriptor.FullName;
            ConfigData = config.ToByteString().ToStringUtf8();
        }
    }
}
