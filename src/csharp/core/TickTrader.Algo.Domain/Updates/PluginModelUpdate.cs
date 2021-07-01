namespace TickTrader.Algo.Domain
{
    public partial class PluginModelUpdate
    {
        public static PluginModelUpdate Added(string id, PluginModelInfo plugin)
        {
            return new PluginModelUpdate { Action = Update.Types.Action.Added, Id = id, Plugin = plugin };
        }

        public static PluginModelUpdate Updated(string id, PluginModelInfo plugin)
        {
            return new PluginModelUpdate { Action = Update.Types.Action.Updated, Id = id, Plugin = plugin };
        }

        public static PluginModelUpdate Removed(string id)
        {
            return new PluginModelUpdate { Action = Update.Types.Action.Removed, Id = id };
        }
    }
}
