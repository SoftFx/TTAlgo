namespace TickTrader.Algo.Domain
{
    public partial class DrawableObjectAnchorsInfo
    {
        public int Count => Price.Count;


        public DrawableObjectAnchorsInfo(int anchorsCount)
        {
            for (var i = 0; i < anchorsCount; i++)
            {
                Price.Add(default(double));
                TimeRaw.Add(UtcTicks.Default.Value);
            }
        }


        public UtcTicks GetTime(int index) => new UtcTicks(TimeRaw[index]);

        public void SetTime(int index, UtcTicks time) => TimeRaw[index] = time.Value;
    }
}
