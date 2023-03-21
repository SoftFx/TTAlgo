namespace TickTrader.Algo.Domain
{
    public partial class DrawableShapePropsInfo
    {
        partial void OnConstruction()
        {
            BorderColorArgb = 0xff008000;
            BorderThickness = 1;
            BorderStyle = Metadata.Types.LineStyle.Solid;
            FillColorArgb = null;
        }
    }
}
