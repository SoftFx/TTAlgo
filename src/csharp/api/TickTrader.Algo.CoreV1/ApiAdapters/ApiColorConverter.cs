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

            Span<uint> intSpan = stackalloc uint[] { (uint)color };
            var byteSpan = MemoryMarshal.Cast<uint, byte>(intSpan);
            if (byteSpan[3] == 0)
                byteSpan[3] = 0xff;

            return intSpan[0];
        }

        public static uint ToArgb(this Colors color, uint autoColor)
        {
            if (color == Colors.Auto)
                return autoColor;

            Span<uint> intSpan = stackalloc uint[] { (uint)color };
            var byteSpan = MemoryMarshal.Cast<uint, byte>(intSpan);
            if (byteSpan[3] == 0)
                byteSpan[3] = 0xff;

            return intSpan[0];
        }

        public static Colors FromArgb(this uint? colorArgb)
        {
            if (!colorArgb.HasValue)
                return Colors.Auto;

            return colorArgb.Value.FromArgb();
        }

        public static Colors FromArgb(this uint colorArgb)
        {
            Span<uint> intSpan = stackalloc uint[] { colorArgb };
            var byteSpan = MemoryMarshal.Cast<uint, byte>(intSpan);
            byteSpan[3] = 0;

            return (Colors)intSpan[0];
        }
    }
}
