namespace TickTrader.Algo.Account.FeedStorage
{
    public struct EntityUpdateArgs<T>
    {
        public EntityUpdateArgs(T val, EntityUpdateActions action)
        {
            Action = action;
            Entity = val;
        }

        public T Entity { get; }
        public EntityUpdateActions Action { get; }
    }

    public enum EntityUpdateActions { Add, Remove, Replace }
}
