namespace TickTrader.Algo.Domain
{
    public static class EnumExtensions
    {
        public static OrderOptions ToOrderOptions(this OrderExecOptions options)
        {
            var res = OrderOptions.None;

            if (options.HasFlag(OrderExecOptions.ImmediateOrCancel))
                res |= OrderOptions.ImmediateOrCancel;

            if (options.HasFlag(OrderExecOptions.OneCancelsTheOther))
                res |= OrderOptions.OneCancelsTheOther;

            return res;
        }

        public static OrderExecOptions ToOrderExecOptions(this OrderOptions options)
        {
            var res = OrderExecOptions.None;

            if (options.HasFlag(OrderOptions.ImmediateOrCancel))
                res |= OrderExecOptions.ImmediateOrCancel;

            if (options.HasFlag(OrderOptions.OneCancelsTheOther))
                res |= OrderExecOptions.OneCancelsTheOther;

            return res;
        }
    }
}
