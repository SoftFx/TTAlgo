using System;
using System.Runtime.InteropServices;
using System.Windows.Media;

namespace TickTrader.BotTerminal
{
    public static class WindowsColorExtensions
    {
        public static Color ToWindowsColor(this uint colorArgb)
        {
            Span<uint> intSpan = stackalloc uint[] { colorArgb };
            var byteSpan = MemoryMarshal.Cast<uint, byte>(intSpan);
            return Color.FromArgb(byteSpan[3], byteSpan[2], byteSpan[1], byteSpan[0]);
        }

        public static uint ToArgb(this Color color)
        {
            Span<uint> intSpan = stackalloc uint[] { 0 };
            var byteSpan = MemoryMarshal.Cast<uint, byte>(intSpan);

            byteSpan[3] = color.A;
            byteSpan[2] = color.R;
            byteSpan[1] = color.G;
            byteSpan[0] = color.B;

            return intSpan[0];
        }
    }
}
