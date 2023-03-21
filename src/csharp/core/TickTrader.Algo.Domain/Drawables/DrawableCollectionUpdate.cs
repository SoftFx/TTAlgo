namespace TickTrader.Algo.Domain
{
    public partial class DrawableCollectionUpdate
    {
        public static DrawableCollectionUpdate Added(DrawableObjectInfo objInfo)
            => Create(CollectionUpdate.Types.Action.Added, objInfo);

        public static DrawableCollectionUpdate Updated(DrawableObjectInfo objInfo)
            => Create(CollectionUpdate.Types.Action.Updated, objInfo);

        public static DrawableCollectionUpdate Removed(string objName)
            => Create(CollectionUpdate.Types.Action.Removed, objName);

        public static DrawableCollectionUpdate Cleared()
            => Create(CollectionUpdate.Types.Action.Cleared, default(string));


        private static DrawableCollectionUpdate Create(CollectionUpdate.Types.Action action, DrawableObjectInfo objInfo)
            => new DrawableCollectionUpdate { Action = action, ObjName = objInfo.Name, ObjInfo = objInfo };

        private static DrawableCollectionUpdate Create(CollectionUpdate.Types.Action action, string objName)
            => new DrawableCollectionUpdate { Action = action, ObjName = objName };
    }
}
