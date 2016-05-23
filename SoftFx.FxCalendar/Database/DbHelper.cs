using System;
using System.IO;

namespace SoftFx.FxCalendar.Database
{
    public static class DbHelper
    {
        public const string DbFileExtension = "db";

        public static string GetDbDirectory(string location)
        {
            return Path.Combine(Directory.GetCurrentDirectory(), location);
        }

        public static string GetDbFilePath(string location, string calendarName, string currencyCode)
        {
            var dbFileName = Uri.EscapeDataString($"{calendarName}-{currencyCode}");
            return Path.Combine(GetDbDirectory(location), $"{dbFileName}.{DbFileExtension}");
        }
    }
}