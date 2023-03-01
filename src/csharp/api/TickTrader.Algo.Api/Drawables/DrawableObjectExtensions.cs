using System;

namespace TickTrader.Algo.Api
{
    public static class DrawableObjectExtensions
    {
        public static IDrawableObject SetCommonProps(this IDrawableObject obj, bool isBackground = false,
            bool isHidden = true, int zIndex = 0)
        {
            obj.IsBackground = isBackground;
            obj.IsHidden = isHidden;
            obj.ZIndex = zIndex;
            return obj;
        }

        public static IDrawableObject SetTooltip(this IDrawableObject obj, string tooltip)
        {
            obj.Tooltip = tooltip;
            return obj;
        }

        public static IDrawableObject SetVisibillity(this IDrawableObject obj, DrawableObjectVisibility visibility)
        {
            obj.Visibility = visibility;
            return obj;
        }

        public static IDrawableObject SetLineProps(this IDrawableObject obj, Colors color,
            ushort thickness = 1, LineStyles style = LineStyles.Solid)
        {
            obj.Line.Color = color;
            obj.Line.Thickness = thickness;
            obj.Line.Style = style;
            return obj;
        }

        public static IDrawableObject SetShapeProps(this IDrawableObject obj, Colors borderColor,
            ushort borderThickness = 1, LineStyles borderStyle = LineStyles.Solid, Colors fillColor = Colors.Auto)
        {
            obj.Shape.BorderColor = borderColor;
            obj.Shape.BorderThickness = borderThickness;
            obj.Shape.BorderStyle = borderStyle;
            obj.Shape.FillColor = fillColor;
            return obj;
        }

        public static IDrawableObject SetSymbolProps(this IDrawableObject obj, Colors color, ushort code, ushort size)
        {
            obj.Symbol.Color = color;
            obj.Symbol.Code = code;
            obj.Symbol.Size = size;
            return obj;
        }

        public static IDrawableObject SetSymbolProps(this IDrawableObject obj, Colors color, ushort code, ushort size,
            DrawableSymbolAnchor anchor = DrawableSymbolAnchor.Bottom, string fontFamily = "Wingdings")
        {
            obj.Symbol.Code = code;
            obj.Symbol.Color = color;
            obj.Symbol.Size = size;
            obj.Symbol.Anchor = anchor;
            obj.Symbol.FontFamily = fontFamily;
            return obj;
        }

        public static IDrawableObject SetTextProps(this IDrawableObject obj, string content, Colors color)
        {
            obj.Text.Content = content;
            obj.Text.Color = color;
            return obj;
        }

        public static IDrawableObject SetTextProps(this IDrawableObject obj, string content, Colors color,
            ushort fontSize = 10, string fontFamily = "Arial")
        {
            obj.Text.Content = content;
            obj.Text.Color = color;
            obj.Text.FontSize = fontSize;
            obj.Text.FontFamily = fontFamily;
            return obj;
        }

        public static IDrawableObject SetControlPosition(this IDrawableObject obj, int x, int y)
        {
            obj.Control.X = x;
            obj.Control.Y = y;
            return obj;
        }

        public static IDrawableObject SetControlPosition(this IDrawableObject obj, int x, int y, DrawableControlZeroPosition zeroPosition)
        {
            obj.Control.X = x;
            obj.Control.Y = y;
            obj.Control.ZeroPosition = zeroPosition;
            return obj;
        }

        public static IDrawableObject SetControlDimensions(this IDrawableObject obj, uint? width, uint? height)
        {
            obj.Control.Width = width;
            obj.Control.Height = height;
            return obj;
        }

        public static IDrawableObject SetControlProps(this IDrawableObject obj,
            DrawablePositionMode contentAlignment = DrawablePositionMode.Center, bool switchState = false, bool readOnly = false)
        {
            obj.Control.ContentAlignment = contentAlignment;
            obj.Control.SwitchState = switchState;
            obj.Control.ReadOnly = readOnly;
            return obj;
        }

        public static IDrawableObject SetBitmapProps(this IDrawableObject obj, int xOffset, int yOffset, uint width, uint height, string filePath)
        {
            obj.Bitmap.OffsetX = xOffset;
            obj.Bitmap.OffsetY = yOffset;
            obj.Bitmap.Width = width;
            obj.Bitmap.Height = height;
            obj.Bitmap.FilePath = filePath;
            return obj;
        }

        public static IDrawableObject SetAnchor(this IDrawableObject obj, int index, DateTime time, double price)
        {
            obj.Anchors[index].Time = time;
            obj.Anchors[index].Price = price;
            return obj;
        }

        public static IDrawableObject SetAnchorList(this IDrawableObject obj, DateTime time)
        {
            obj.Anchors[0].Time = time;
            return obj;
        }

        public static IDrawableObject SetAnchorList(this IDrawableObject obj, double price)
        {
            obj.Anchors[0].Price = price;
            return obj;
        }

        public static IDrawableObject SetAnchorList(this IDrawableObject obj, DateTime time, double price)
        {
            return obj.SetAnchor(0, time, price);
        }

        public static IDrawableObject SetAnchorList(this IDrawableObject obj, DateTime time0, double price0, DateTime time1, double price1)
        {
            return obj.SetAnchor(0, time0, price0).SetAnchor(1, time1, price1);
        }

        public static IDrawableObject SetAnchorList(this IDrawableObject obj, DateTime time0, double price0, DateTime time1, double price1, DateTime time2, double price2)
        {
            return obj.SetAnchor(0, time0, price0).SetAnchor(1, time1, price1).SetAnchor(2, time2, price2);
        }

        public static IDrawableObject ConfigureLevels(this IDrawableObject obj, int levelsCnt, Colors defaultColor)
        {
            obj.Levels.Count = levelsCnt;
            obj.Levels.DefaultColor = defaultColor;
            return obj;
        }

        public static IDrawableObject ConfigureLevels(this IDrawableObject obj, int levelsCnt, Colors defaultColor,
            ushort defaultLineThickness = 1, LineStyles defaultLineStyle = LineStyles.Solid, string defaultFontFamily = "Arial", ushort defaultFontSize = 10)
        {
            obj.Levels.Count = levelsCnt;
            obj.Levels.DefaultColor = defaultColor;
            obj.Levels.DefaultLineThickness = defaultLineThickness;
            obj.Levels.DefaultLineStyle = defaultLineStyle;
            obj.Levels.DefaultFontFamily = defaultFontFamily;
            obj.Levels.DefaultFontSize = defaultFontSize;
            return obj;
        }

        public static IDrawableObject SetLevelProps(this IDrawableObject obj, int index, double value, string text)
        {
            obj.Levels[index].Value = value;
            obj.Levels[index].Text = text;
            return obj;
        }

        public static IDrawableObject SetLevelProps(this IDrawableObject obj, int index, double value, string text,
            Colors color = Colors.Auto, ushort? lineThickness = null, LineStyles? lineStyle = null, string fontFamily = null, ushort? fontSize = null)
        {
            obj.Levels[index].Value = value;
            obj.Levels[index].Text = text;
            obj.Levels[index].Color = color;
            obj.Levels[index].LineThickness = lineThickness;
            obj.Levels[index].LineStyle = lineStyle;
            obj.Levels[index].FontFamily = fontFamily;
            obj.Levels[index].FontSize = fontSize;
            return obj;
        }

        //public static IDrawableObject SetSpecialProps(this IDrawableObject obj, double? angle = null, double? scale = null, double? deviation = null,
        //    DrawableLineRayMode rayMode = DrawableLineRayMode.RayNone, bool fill = false, bool fiboArcsFullEllipse = false,
        //    DrawablePositionMode anchorPosition = DrawablePositionMode.TopLeft, DrawableGannDirection gannDirection = DrawableGannDirection.UpTrend)
        //{
        //    obj.Special.Angle = angle;
        //    obj.Special.Scale = scale;
        //    obj.Special.Deviation = deviation;
        //    obj.Special.RayMode = rayMode;
        //    obj.Special.Fill = fill;
        //    obj.Special.FiboArcsFullEllipse = fiboArcsFullEllipse;
        //    obj.Special.AnchorPosition = anchorPosition;
        //    obj.Special.GannDirection = gannDirection;
        //    return obj;
        //}

        // SetCommonProps
        // SetVisibillity

        public static IDrawableObject SetAngle(this IDrawableObject obj, double? angle)
        {
            obj.Special.Angle = angle;
            return obj;
        }

        public static IDrawableObject SetScale(this IDrawableObject obj, double? scale)
        {
            obj.Special.Scale = scale;
            return obj;
        }

        public static IDrawableObject SetDeviation(this IDrawableObject obj, double? deviation)
        {
            obj.Special.Deviation = deviation;
            return obj;
        }

        public static IDrawableObject SetRayMode(this IDrawableObject obj, DrawableLineRayMode rayMode)
        {
            obj.Special.RayMode = rayMode;
            return obj;
        }

        public static IDrawableObject SetFill(this IDrawableObject obj, bool fill)
        {
            obj.Special.Fill = fill;
            return obj;
        }

        public static IDrawableObject SetFiboArcsFullEllipse(this IDrawableObject obj, bool fullEllipse)
        {
            obj.Special.FiboArcsFullEllipse = fullEllipse;
            return obj;
        }

        public static IDrawableObject SetAnchorPosition(this IDrawableObject obj, DrawablePositionMode position)
        {
            obj.Special.AnchorPosition = position;
            return obj;
        }

        public static IDrawableObject SetGannDirection(this IDrawableObject obj, DrawableGannDirection direction)
        {
            obj.Special.GannDirection = direction;
            return obj;
        }
    }
}
