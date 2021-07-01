namespace TickTrader.Algo.Domain
{
    public partial class AccountModelUpdate
    {
        public static AccountModelUpdate Added(string id, AccountModelInfo account)
        {
            return new AccountModelUpdate { Action = Update.Types.Action.Added, Id = id, Account = account };
        }

        public static AccountModelUpdate Updated(string id, AccountModelInfo account)
        {
            return new AccountModelUpdate { Action = Update.Types.Action.Updated, Id = id, Account = account };
        }

        public static AccountModelUpdate Removed(string id)
        {
            return new AccountModelUpdate { Action = Update.Types.Action.Removed, Id = id };
        }
    }
}
