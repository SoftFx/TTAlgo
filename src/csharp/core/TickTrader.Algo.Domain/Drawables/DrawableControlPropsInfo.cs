namespace TickTrader.Algo.Domain
{
    public partial class DrawableControlPropsInfo
    {
        partial void OnConstruction()
        {
            X = 0; Y = 0;
            Width = 50; Height = 50;
            ZeroPosition = Drawable.Types.ControlZeroPosition.LowerLeft;
            ContentAlignment = Drawable.Types.PositionMode.Center;
        }
    }
}
