using System;
using System.IO;
using System.Text.RegularExpressions;

namespace TickTrader.DedicatedServer.Extensions
{
    public static class PathExtensions
    {
        static readonly string invalidChars = @"""\/?:<>*|";
        static readonly string escapeChar = "%";

        static readonly Regex escaper = new Regex(
            "[" + Regex.Escape(escapeChar + invalidChars) + "]",
            RegexOptions.Compiled);

        static readonly Regex unescaper = new Regex(
            Regex.Escape(escapeChar) + "([0-9A-Z]{4})",
            RegexOptions.Compiled);

        public static string Escape(this string path)
        {
            return escaper.Replace(path,
                m => escapeChar + ((short)(m.Value[0])).ToString("X4"));
        }

        public static string Unescape(this string path)
        {
            return unescaper.Replace(path,
                m => ((char)Convert.ToInt16(m.Groups[1].Value, 16)).ToString());
        }

        public static bool IsPathAbsolute(this string path)
        {
            return path == Path.GetFullPath(path);
        }

        public static bool IsFileNameValid(this string fileName)
        {
            return !Regex.IsMatch(fileName, $"[{invalidChars}]");
        }
    }
}
