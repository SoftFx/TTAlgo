﻿namespace TickTrader.Algo.Domain
{
    public partial class DrawableSpecialPropsInfo
    {
        partial void OnConstruction()
        {
            RayMode = Drawable.Types.LineRayMode.RayNone;
            GannDirection = Drawable.Types.GannDirection.UpTrend;
            AnchorPosition = Drawable.Types.PositionMode.TopLeft;
        }
    }
}
