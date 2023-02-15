namespace TickTrader.Algo.Domain
{
    public partial class DrawableControlPropsInfo
    {
        partial void OnConstruction()
        {
            X = 0; Y = 0;
            Width = 50; Height = 50;
            Anchor = Drawable.Types.ControlAnchor.LowerLeft;
        }
    }
}
