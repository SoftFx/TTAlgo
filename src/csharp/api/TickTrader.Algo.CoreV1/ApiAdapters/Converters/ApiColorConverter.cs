using System;
using System.Runtime.InteropServices;
using TickTrader.Algo.Api;

namespace TickTrader.Algo.CoreV1
{
    public static class ApiColorConverter
    {
        public static readonly uint GreenColor = Colors.Green.ToArgb().Value;


        public static uint? ToArgb(this Colors color)
        {
            if (color == Colors.Auto)
                return null;

            return ToArgbInternal(color);
        }

        public static uint ToArgb(this Colors color, uint autoColor)
        {
            if (color == Colors.Auto)
                return autoColor;

            return ToArgbInternal(color);
        }

        public static Colors FromArgb(this uint? colorArgb)
        {
            if (!colorArgb.HasValue)
                return Colors.Auto;

            return FromArgbInternal(colorArgb.Value);
        }

        public static Colors FromArgb(this uint colorArgb)
        {
            return FromArgbInternal(colorArgb);
        }


        private static uint ToArgbInternal(Colors color)
        {
            Span<uint> intSpan = stackalloc uint[] { (uint)color };
            var byteSpan = MemoryMarshal.Cast<uint, byte>(intSpan);
            if (byteSpan[3] == 0)
                byteSpan[3] = 0xff;
            else if (byteSpan[3] == 0xff)
                byteSpan[3] = 0;

            return intSpan[0];
        }

        private static Colors FromArgbInternal(uint colorArgb)
        {
            Span<uint> intSpan = stackalloc uint[] { colorArgb };
            var byteSpan = MemoryMarshal.Cast<uint, byte>(intSpan);
            if (byteSpan[3] == 0xff)
                byteSpan[3] = 0;
            else if (byteSpan[3] == 0)
                byteSpan[3] = 0xff;

            return (Colors)intSpan[0];
        }
    }
}
