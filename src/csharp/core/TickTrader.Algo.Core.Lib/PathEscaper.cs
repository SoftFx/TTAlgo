using System;
using System.Text.RegularExpressions;

namespace TickTrader.Algo.Core.Lib
{
    public static class PathEscaper
    {
        public const string InvalidChars = @"""\/?:<>*|";
        public const string EscapeChar = "%";

        private static readonly Regex _escaper = new Regex(
            "[" + Regex.Escape(EscapeChar + InvalidChars) + "]",
            RegexOptions.Compiled);

        private static readonly Regex _unescaper = new Regex(
            Regex.Escape(EscapeChar) + "([0-9A-Z]{4})",
            RegexOptions.Compiled);

        public static string Escape(string path)
        {
            return _escaper.Replace(path,
                m => EscapeChar + ((short)(m.Value[0])).ToString("X4"));
        }

        public static string Unescape(string path)
        {
            return _unescaper.Replace(path,
                m => ((char)Convert.ToInt16(m.Groups[1].Value, 16)).ToString());
        }
    }
}
