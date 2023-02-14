namespace TickTrader.Algo.Domain
{
    public partial class DrawableLinePropsInfo
    {
        partial void OnConstruction()
        {
            ColorArgb = 0xff008000;
            Thickness = 1;
            Style = Metadata.Types.LineStyle.Solid;
            RayLeft = RayRight = RayVertical = false;
        }
    }
}
