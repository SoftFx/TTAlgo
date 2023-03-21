namespace TickTrader.Algo.Domain
{
    public partial class DrawableAnchorPropsInfo
    {
        public UtcTicks Time
        {
            get => new UtcTicks(TimeRaw);
            set => TimeRaw = value.Value;
        }


        partial void OnConstruction()
        {
            Price = 0.0;
            TimeRaw = UtcTicks.Default.Value;
        }
    }


    public partial class DrawableObjectAnchorsList
    {
        public int Count => Anchors.Count;

        public DrawableAnchorPropsInfo this[int index] => Anchors[index];


        public DrawableObjectAnchorsList(int cnt)
        {
            for (var i = 0; i < cnt; i++)
                Anchors.Add(new DrawableAnchorPropsInfo());
        }
    }
}
