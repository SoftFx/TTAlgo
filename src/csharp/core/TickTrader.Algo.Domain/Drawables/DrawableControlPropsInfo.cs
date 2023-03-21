namespace TickTrader.Algo.Domain
{
    public partial class DrawableControlPropsInfo
    {
        partial void OnConstruction()
        {
            ZeroPosition = Drawable.Types.ControlZeroPosition.LowerLeft;
            ContentAlignment = Drawable.Types.PositionMode.Center;
        }
    }
}
