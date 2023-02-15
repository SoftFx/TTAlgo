namespace TickTrader.Algo.Domain
{
    public partial class DrawableObjectLevelsInfo
    {
        public int Count => Value.Count;


        public DrawableObjectLevelsInfo(int levelsCount)
        {
            for (var i =0; i < levelsCount; i++)
            {
                Value.Add(default(double));
                Width.Add(default(int));
                ColorArgb.Add(0xff008000);
                LineStyle.Add(Metadata.Types.LineStyle.Solid);
                Description.Add(default(string));
            }
        }
    }
}
