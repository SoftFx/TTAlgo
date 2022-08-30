namespace TickTrader.Algo.Domain
{
    public partial class ContingentOrderTrigger
    {
        public UtcTicks? TriggerTime
        {
            get => TriggerTimeTicks.ToUtcTicks();
            set => TriggerTimeTicks = value.ToInt64();
        }
    }
}
