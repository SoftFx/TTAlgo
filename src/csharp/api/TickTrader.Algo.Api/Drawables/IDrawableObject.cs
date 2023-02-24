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
        int ZIndex { get; set; }
        string Tooltip { get; set; }
        DrawableObjectVisibility Visibility { get; set; }


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
        ushort Thickness { get; set; }
        LineStyles Style { get; set; }
    }

    public interface IDrawableShapeProps
    {
        bool IsSupported { get; }
        Colors BorderColor { get; set; }
        ushort BorderThickness { get; set; }
        LineStyles BorderStyle { get; set; }
        Colors FillColor { get; set; }
    }

    public static class DrawableSymbolSpecialCodes
    {
        public const ushort Buy = 1;
        public const ushort Sell = 2;
        public const ushort LeftPriceLabel = 3;
        public const ushort RightPriceLabel = 4;
        public const ushort ThumbsUp = 67;
        public const ushort ThumbsDown= 68;
        public const ushort ArrowUp = 241;
        public const ushort ArrowDown = 242;
        public const ushort StopSign = 251;
        public const ushort CheckSign = 252;
    }

    public interface IDrawableSymbolProps
    {
        bool IsSupported { get; }
        ushort Code { get; set; }
        ushort Size { get; set; }
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
        ushort FontSize { get; set; }
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
        ushort? LineThickness { get; set; }
        LineStyles? LineStyle { get; set; }
        string FontFamily { get; set; }
        ushort? FontSize { get; set; }
    }

    public interface IDrawableObjectLevels
    {
        bool IsSupported { get; }
        int Count { get; set; }
        IDrawableLevelProps this[int index] { get; }

        Colors DefaultColor { get; set; }
        ushort DefaultLineThickness { get; set; }
        LineStyles DefaultLineStyle { get; set; }
        string DefaultFontFamily { get; set; }
        ushort DefaultFontSize { get; set; }
    }

    public interface IDrawableControlProps
    {
        bool IsSupported { get; }
        int X { get; set; }
        int Y { get; set; }
        uint? Width { get; set; }
        uint? Height { get; set; }
        DrawableControlZeroPosition ZeroPosistion { get; set; }
        DrawablePositionMode ContentAlignment { get; set; }
        bool SwitchState { get; set; }
        bool ReadOnly { get; set; }
    }

    public interface IDrawableBitmapProps
    {
        bool IsSupported { get; }
        int OffsetX { get; set; }
        int OffsetY { get; set; }
        uint Width { get; set; }
        uint Height { get; set; }
        string FilePath { get; set; }
    }

    public interface IDrawableSpecialProps
    {
        bool IsSupported { get; }

        double? Angle { get; set; }
        double? Scale { get; set; }
        double? Deviation { get; set; }

        DrawableLineRayMode RayMode { get; set; }
        bool Fill { get; set; }
        bool FiboArcsFullEllipse { get; set; }
        DrawablePositionMode AnchorPosition { get; set; }
        DrawableGannDirection GannDirection { get; set; }
    }
}
