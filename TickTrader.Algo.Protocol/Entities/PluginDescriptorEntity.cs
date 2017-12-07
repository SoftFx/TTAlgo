using SoftFX.Net.BotAgent;

namespace TickTrader.Algo.Protocol
{
    public enum PluginType
    {
        Indicator,
        Robot,
        Unknown,
    }


    public class PluginDescriptorEntity
    {
        public string ApiVersion { get; set; }

        public string Id { get; set; }

        public string DisplayName { get; set; }

        public string UserDisplayName { get; set; }

        public string Category { get; set; }

        public string Version { get; set; }

        public string Description { get; set; }

        public string Copyright { get; set; }

        public PluginType Type { get; set; }


        internal void UpdateModel(PluginDescriptor model)
        {
            model.ApiVersion = ApiVersion;
            model.Id = Id;
            model.DisplayName = DisplayName;
            model.UserDisplayName = UserDisplayName;
            model.Category = Category;
            model.Version = Version;
            model.Description = Description;
            model.Copyright = Copyright;
            model.Type = ToSfx.Convert(Type);
        }

        internal void UpdateSelf(PluginDescriptor model)
        {
            ApiVersion = model.ApiVersion;
            Id = model.Id;
            DisplayName = model.DisplayName;
            UserDisplayName = model.UserDisplayName;
            Category = model.Category;
            Version = model.Version;
            Description = model.Description;
            Copyright = model.Copyright;
            Type = ToAlgo.Convert(model.Type);
        }
    }
}
