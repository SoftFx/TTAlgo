namespace TickTrader.Algo.Domain
{
    public static class EnumExtensions
    {
        public static OrderOptions ToOrderOptions(this OrderExecOptions options)
        {
            var res = OrderOptions.None;

            if (options.HasFlag(OrderExecOptions.ImmediateOrCancel))
                res |= OrderOptions.ImmediateOrCancel;

            return res;
        }

        public static OrderExecOptions ToOrderExecOptions(this OrderOptions options)
        {
            var res = OrderExecOptions.None;

            if (options.HasFlag(OrderOptions.ImmediateOrCancel))
                res |= OrderExecOptions.ImmediateOrCancel;

            return res;
        }
    }
}
