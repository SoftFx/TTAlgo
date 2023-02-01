namespace TickTrader.Algo.Domain
{
    public partial class DrawableLinePropsInfo
    {
        partial void OnConstruction()
        {
            Thickness = 1;
            RayLeft = RayRight = RayVertical = false;
        }
    }
}
