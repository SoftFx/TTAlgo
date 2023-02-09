using System;
using TickTrader.Algo.Api;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.CoreV1
{
    internal static class DrawableEnumConverter
    {
        public static Drawable.Types.ObjectType ToDomain(this DrawableObjectType type)
        {
            switch (type)
            {
                //case DrawableObjectType.Unknown: return Drawable.Types.ObjectType.UnknownObjectType;
                case DrawableObjectType.VerticalLine:return Drawable.Types.ObjectType.VerticalLine;
                case DrawableObjectType.HorizontalLine: return Drawable.Types.ObjectType.VerticalLine;
                case DrawableObjectType.TrendLine: return Drawable.Types.ObjectType.VerticalLine;
                case DrawableObjectType.Rectangle: return Drawable.Types.ObjectType.VerticalLine;
                case DrawableObjectType.Triangle: return Drawable.Types.ObjectType.VerticalLine;
                case DrawableObjectType.Ellipse: return Drawable.Types.ObjectType.VerticalLine;
                case DrawableObjectType.Symbol: return Drawable.Types.ObjectType.VerticalLine;
                case DrawableObjectType.Text: return Drawable.Types.ObjectType.VerticalLine;
                case DrawableObjectType.Bitmap: return Drawable.Types.ObjectType.VerticalLine;
                case DrawableObjectType.LabelControl: return Drawable.Types.ObjectType.VerticalLine;
                case DrawableObjectType.RectangleControl: return Drawable.Types.ObjectType.VerticalLine;
                case DrawableObjectType.EditControl: return Drawable.Types.ObjectType.VerticalLine;
                case DrawableObjectType.ButtonControl: return Drawable.Types.ObjectType.VerticalLine;
                case DrawableObjectType.BitmapControl: return Drawable.Types.ObjectType.VerticalLine;
                default: throw new ArgumentException("Unknown object type");
            }
        }
    }
}
