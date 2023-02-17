using System;

namespace TickTrader.Algo.Api
{
    public interface IDrawableObject
    {
        string Name { get; }
        DrawableObjectType Type { get; }
        DateTime CreatedTime { get; }
        OutputTargets TargetWindow { get; }


        bool IsBackground { get; set; }
        bool IsHidden { get; set; }
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
        IDrawableSpecialProps Special { get; }


        void PushChanges();
    }

    public interface IDrawableLineProps
    {
        bool IsSupported { get; }
        Colors Color { get; set; }
        int Thickness { get; set; }
        LineStyles Style { get; set; }
    }

    public interface IDrawableShapeProps
    {
        bool IsSupported { get; }
        Colors BorderColor { get; set; }
        int BorderThickness { get; set; }
        LineStyles BorderStyle { get; set; }
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
    }

    public interface IDrawableAnchorProps
    {
        DateTime Time { get; set; }
        double Price { get; set; }
    }

    public interface IDrawableObjectAnchors
    {
        bool IsSupported { get; }
        int Count { get; }
        IDrawableAnchorProps this[int index] { get; }
    }

    public interface IDrawableLevelProps
    {
        double Value { get; set; }
        string Text { get; set; }
        Colors Color { get; set; }
        int? LineThickness { get; set; }
        LineStyles? LineStyle { get; set; }
        string FontFamily { get; set; }
        int? FontSize { get; set; }
    }

    public interface IDrawableObjectLevels
    {
        bool IsSupported { get; }
        int Count { get; set; }
        IDrawableLevelProps this[int index] { get; }

        Colors DefaultColor { get; set; }
        int DefaultLineThickness { get; set; }
        LineStyles DefaultLineStyle { get; set; }
        string DefaultFontFamily { get; set; }
        int DefaultFontSize { get; set; }
    }

    public enum DrawableControlAnchor
    {
        UpperLeft = 0,
        UpperRight = 1,
        LowerLeft = 2,
        LowerRight = 3,
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
        int OffsetX { get; set; }
        int OffsetY { get; set; }
        int Width { get; set; }
        int Height { get; set; }
        string FilePath { get; set; }
    }

    public interface IDrawableSpecialProps
    {
        double Angle { get; set; }
        double Scale { get; set; }
        double Deviation { get; set; }

        bool RayLeft { get; set; }
        bool RayRight { get; set; }
        bool RayVertical { get; set; }
        bool Fill { get; set; }
        bool ButtonState { get; set; }
    }
}
