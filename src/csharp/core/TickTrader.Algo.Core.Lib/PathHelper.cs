using System;
using System.IO;
using System.Text.RegularExpressions;

namespace TickTrader.Algo.Core.Lib
{
    public static class PathHelper
    {
        public const string InvalidChars = @"""\/?:<>*|";
        public const string EscapeChar = "%";


        private static readonly Regex _escaper = new Regex(
            "[" + Regex.Escape(EscapeChar + InvalidChars) + "]", RegexOptions.Compiled);

        private static readonly Regex _unescaper = new Regex(
            Regex.Escape(EscapeChar) + "([0-9A-Z]{4})", RegexOptions.Compiled);

        private static readonly Regex _fileNameValidator = new Regex(
            $"[{InvalidChars}]", RegexOptions.Compiled);


        public static string EnsureDirectoryCreated(string path)
        {
            Directory.CreateDirectory(path); //CreateDirectory already include Directory.Exist

            return path;
        }

        public static bool IsPathAbsolute(string path)
        {
            return path == Path.GetFullPath(path);
        }

        public static bool IsFileNameValid(string fileName)
        {
            return !_fileNameValidator.IsMatch(fileName);
        }

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

        public static string GenerateUniqueFilePath(string pathPrefix, string ext)
        {
            var cnt = 0;
            var path = pathPrefix + ext;

            while (File.Exists(path) && cnt < int.MaxValue)
            {
                path = $"{pathPrefix}{++cnt}{ext}";
            }

            if (File.Exists(path))
                path = $"{pathPrefix}{DateTime.UtcNow.Ticks}{ext}";

            return path;
        }
    }
}
