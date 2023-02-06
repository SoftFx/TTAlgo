namespace TickTrader.Algo.Domain
{
    public partial class DrawableObjectUpdate
    {
        public static DrawableObjectUpdate Added(DrawableObjectInfo info)
            => new DrawableObjectUpdate { Action = Update.Types.Action.Added, Name = info.Name, Info = info };

        public static DrawableObjectUpdate Updated(DrawableObjectInfo info)
            => new DrawableObjectUpdate { Action = Update.Types.Action.Updated, Name = info.Name, Info = info };

        public static DrawableObjectUpdate Removed(string name)
            => new DrawableObjectUpdate { Action = Update.Types.Action.Removed, Name = name };
    }
}
