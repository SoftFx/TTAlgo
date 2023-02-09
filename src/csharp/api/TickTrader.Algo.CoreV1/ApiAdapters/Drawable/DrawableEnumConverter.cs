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
                case DrawableObjectType.HorizontalLine: return Drawable.Types.ObjectType.HorizontalLine;
                case DrawableObjectType.TrendLine: return Drawable.Types.ObjectType.TrendLine;
                case DrawableObjectType.Rectangle: return Drawable.Types.ObjectType.Rectangle;
                case DrawableObjectType.Triangle: return Drawable.Types.ObjectType.Triangle;
                case DrawableObjectType.Ellipse: return Drawable.Types.ObjectType.Ellipse;
                case DrawableObjectType.Symbol: return Drawable.Types.ObjectType.Symbol;
                case DrawableObjectType.Text: return Drawable.Types.ObjectType.Text;
                case DrawableObjectType.Bitmap: return Drawable.Types.ObjectType.Bitmap;
                case DrawableObjectType.LabelControl: return Drawable.Types.ObjectType.LabelControl;
                case DrawableObjectType.RectangleControl: return Drawable.Types.ObjectType.RectangleControl;
                case DrawableObjectType.EditControl: return Drawable.Types.ObjectType.EditControl;
                case DrawableObjectType.ButtonControl: return Drawable.Types.ObjectType.ButtonControl;
                case DrawableObjectType.BitmapControl: return Drawable.Types.ObjectType.BitmapControl;
                default: throw new ArgumentException("Unknown object type");
            }
        }
    }
}
