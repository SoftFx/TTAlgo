namespace TickTrader.Algo.Domain
{
    public partial class DrawableSpecialPropsInfo
    {
        partial void OnConstruction()
        {
            GannDirection = Drawable.Types.GannDirection.UpTrend;
            AnchorPosition = Drawable.Types.PositionMode.TopLeft;
        }
    }
}
