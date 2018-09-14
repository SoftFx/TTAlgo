using SoftFX.Net.BotAgent;
using System;

namespace TickTrader.Algo.Protocol.Sfx
{
    public class PackageModelEntity
    {
        public string Name { get; set; }

        public DateTime Created { get; set; }

        public PluginInfoEntity[] Plugins { get; set; }


        public PackageModelEntity()
        {
            Plugins = new PluginInfoEntity[0];
        }


        internal void UpdateModel(PackageModel model)
        {
            model.Name = Name;
            model.Created = Created;
            model.Plugins.Resize(Plugins.Length);
            for (var i = 0; i < Plugins.Length; i++)
            {
                Plugins[i].UpdateModel(model.Plugins[i]);
            }
        }

        internal void UpdateSelf(PackageModel model)
        {
            Name = model.Name;
            Created = model.Created;
            Plugins = new PluginInfoEntity[model.Plugins.Length];
            for (var i = 0; i < model.Plugins.Length; i++)
            {
                Plugins[i] = new PluginInfoEntity();
                Plugins[i].UpdateSelf(model.Plugins[i]);
            }
        }
    }
}
