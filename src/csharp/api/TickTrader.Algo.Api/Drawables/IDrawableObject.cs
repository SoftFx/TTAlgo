﻿using System;

namespace TickTrader.Algo.Api
{
    public interface IDrawableObject
    {
        string Name { get; }

        DrawableObjectType Type { get; }

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
        bool RayLeft { get; set; }
        bool RayRight { get; set; }
        bool RayVertical { get; set; }
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
        void SetTime(int index, DateTime time);
        double GetPrice(int index);
        void SetPrice(int index, double price);
    }

    public interface IDrawableObjectLevels
    {
        bool IsSupported { get; }
        int Count { get; }


        double GetValue(int index);
        void SetValue(int index, double value);
        int GetWidth(int index);
        void SetWidth(int index, int width);
        Colors GetColor(int index);
        void SetColor(int index, Colors color);
        LineStyles GetLineStyle(int index);
        void SetLineStyle(int index, LineStyles style);
        string GetDescription(int index);
        void SetDescription(int index, string description);
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
}
