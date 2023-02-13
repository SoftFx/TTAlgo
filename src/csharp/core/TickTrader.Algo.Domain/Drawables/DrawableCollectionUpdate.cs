namespace TickTrader.Algo.Domain
{
    public partial class DrawableCollectionUpdate
    {
        public static DrawableCollectionUpdate Added(DrawableObjectInfo objInfo)
            => new DrawableCollectionUpdate { Action = CollectionUpdate.Types.Action.Added, ObjName = objInfo.Name, ObjInfo = objInfo };

        public static DrawableCollectionUpdate Updated(DrawableObjectInfo objInfo)
            => new DrawableCollectionUpdate { Action = CollectionUpdate.Types.Action.Updated, ObjName = objInfo.Name, ObjInfo = objInfo };

        public static DrawableCollectionUpdate Removed(string objName)
            => new DrawableCollectionUpdate { Action = CollectionUpdate.Types.Action.Removed, ObjName = objName };

        public static DrawableCollectionUpdate Cleared()
            => new DrawableCollectionUpdate { Action = CollectionUpdate.Types.Action.Cleared };
    }
}
