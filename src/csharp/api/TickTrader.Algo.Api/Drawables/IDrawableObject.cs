using System;

namespace TickTrader.Algo.Api.Drawables
{
    public interface IDrawableObject
    {
        string Name { get; }

        DateTime CreatedTime { get; }

        string OutputId { get; }

        bool IsBackground { get; set; }

        bool IsHidden { get; set; }

        bool IsSelectable { get; set; }

        long ZIndex { get; set; }

        string Tooltip { get; set; }


        IDrawableLineProps Line { get; }
        IDrawableShapeProps Shape { get; }
        IDrawableSymbolProps Symbol { get; }
        IDrawableTextProps Text { get; }
        IDrawableObjectAnchors Anchors { get; }
        IDrawableObjectLevels Levels { get; }
        IDrawableControlProps Control { get; }
        IDrawableBitmapProps Bitmap { get; }


        void PushChanges();
    }

    public interface IDrawableLineProps
    {
        bool IsSupported { get; }
        Colors Color { get; set; }
        int Thickness { get; set; }
        LineStyles Style { get; set; }
        bool Ray { get; set; }
        bool RayLeft { get; set; }
        bool RayRight { get; set; }
    }

    public interface IDrawableShapeProps
    {
        bool IsSupported { get; }
        Colors BorderColor { get; set; }
        int BorderThickness { get; set; }
        LineStyles BorderStyle { get; set; }
        bool Fill { get; set; }
        Colors FillColor { get; set; }
    }

    public enum DrawableSymbolAnchor { Top, Bottom }

    public static class DrawableSymbolSpecialCodes
    {
        public const ushort Buy = 1;
        public const ushort Sell = 2;
        public const ushort LeftPriceLabel = 3;
        public const ushort RightPriceLabel = 4;
    }

    public interface IDrawableSymbolProps
    {
        bool IsSupported { get; }
        ushort Code { get; set; }
        int Size { get; set; }
        Colors Color { get; set; }
        string FontFamily { get; set; } // default font here - "Wingdings"
        DrawableSymbolAnchor Anchor { get; set; }
    }

    public interface IDrawableTextProps
    {
        bool IsSupported { get; }
        string Content { get; set; }
        Colors Color { get; set; }
        string FontFamily { get; set; }
        int FontSize { get; set; }
        double Angle { get; set; }
    }

    public interface IDrawableObjectAnchors
    {
        bool IsSupported { get; }
        int Count { get; }


        DateTime GetTime(int index);
        void SetTime(int index, DateTime value);
        double GetPrice(int index);
        void SetPrice(int index, double price);
    }

    public interface IDrawableObjectLevels
    {
        bool IsSupported { get; }
        int Count { get; }


        double GetValue(int index);
        double SetValue(int index, double value);
        int GetWidth(int index);
        void SetWidth(int index, int width);
        Colors GetColor(int index);
        void SetColor(Colors color);
        LineStyles GetLineStyle(int index);
        void SetLineStyle(int index, LineStyles style);
        string GetDescription(int index);
        void SetDescription(int index, string description);
    }

    public enum DrawableControlAnchor
    {
        UpperLeft = 0,
        CenterLeft = 1,
        LowerLeft = 2,
        UpperCenter = 3,
        Center = 4,
        LowerCenter = 5,
        UpperRight = 6,
        CenterRight = 7,
        LowerRight = 8,
    }

    public interface IDrawableControlProps
    {
        bool IsSupported { get; }
        int X { get; set; }
        int Y { get; set; }
        int Width { get; set; }
        int Height { get; set; }
        DrawableControlAnchor Anchor { get; set; }
    }

    public interface IDrawableBitmapProps
    {
        bool IsSupported { get; }
        int OffsetX { get; }
        int OffsetY { get; }
        string FilePath { get; }
    }
}
